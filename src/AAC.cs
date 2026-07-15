/*
===============================================================================
 Adaptive Antigravity Controller (AAC)
 Version: 0.3.5
 Project Lead: Nomaddison
 Development Assistance: OpenAI ChatGPT

 MILESTONE 3.5 ENGINEERING CONSOLE REFINEMENT
 Self-contained Space Engineers Programmable Block script.  Controller remains
 monitor-only: no propulsion, alarm, or lighting outputs are written.
===============================================================================
*/

    const string Version = "0.3.5";
    const string SystemTag = "[AAC]";

    readonly Configuration _configuration;
    readonly EventLogger _eventLogger;
    readonly HardwareDiscovery _hardwareDiscovery;
    readonly PhysicsEngineModelBuilder _pemBuilder;
    readonly CapabilityAnalysis _capabilityAnalysis;
    readonly DebugManager _debugManager;
    readonly DisplayManager _displayManager;
    readonly PerformanceMonitor _performanceMonitor;
    readonly AacCore _core;

    public Program()
    {
        Runtime.UpdateFrequency = UpdateFrequency.Update100;
        _configuration = new Configuration(SystemTag);
        _eventLogger = new EventLogger(24);
        _hardwareDiscovery = new HardwareDiscovery(GridTerminalSystem, Me, _configuration);
        _pemBuilder = new PhysicsEngineModelBuilder();
        _capabilityAnalysis = new CapabilityAnalysis();
        _debugManager = new DebugManager();
        _displayManager = new DisplayManager(GridTerminalSystem, Me, _configuration, _eventLogger, _debugManager, Echo);
        _performanceMonitor = new PerformanceMonitor();
        _core = new AacCore(Version, _hardwareDiscovery, _pemBuilder, _capabilityAnalysis, _debugManager, _displayManager, _eventLogger, _performanceMonitor);
        _eventLogger.Record(0, "AAC boot: Milestone 3 monitor-only PEM online.");
        _core.Tick("boot");
    }

    public void Main(string argument, UpdateType updateSource)
    {
        _core.Tick(argument == null ? string.Empty : argument.Trim());
    }

    enum ShipAxis { Forward = 0, Backward = 1, Left = 2, Right = 3, Up = 4, Down = 5 }
    enum ValidationState { Ready, NotTagged, Disabled, NotWorking, InvalidFrame }

    sealed class AacCore
    {
        readonly string _version;
        readonly HardwareDiscovery _hardwareDiscovery;
        readonly PhysicsEngineModelBuilder _pemBuilder;
        readonly CapabilityAnalysis _capabilityAnalysis;
        readonly DebugManager _debugManager;
        readonly DisplayManager _displayManager;
        readonly EventLogger _eventLogger;
        readonly PerformanceMonitor _performanceMonitor;
        int _tickCount;

        public AacCore(string version, HardwareDiscovery hardwareDiscovery, PhysicsEngineModelBuilder pemBuilder, CapabilityAnalysis capabilityAnalysis, DebugManager debugManager, DisplayManager displayManager, EventLogger eventLogger, PerformanceMonitor performanceMonitor)
        {
            _version = version;
            _hardwareDiscovery = hardwareDiscovery;
            _pemBuilder = pemBuilder;
            _capabilityAnalysis = capabilityAnalysis;
            _debugManager = debugManager;
            _displayManager = displayManager;
            _eventLogger = eventLogger;
            _performanceMonitor = performanceMonitor;
        }

        public void Tick(string command)
        {
            _tickCount++;
            DateTime scanStart = DateTime.UtcNow;
            HardwareSnapshot hardware = _hardwareDiscovery.Scan();
            double scanMs = (DateTime.UtcNow - scanStart).TotalMilliseconds;
            DateTime pemStart = DateTime.UtcNow;
            PhysicsEngineModel pem = _pemBuilder.Build(hardware);
            double pemMs = (DateTime.UtcNow - pemStart).TotalMilliseconds;
            DateTime capStart = DateTime.UtcNow;
            CapabilitySnapshot capability = _capabilityAnalysis.Evaluate(pem);
            double capMs = (DateTime.UtcNow - capStart).TotalMilliseconds;
            _performanceMonitor.Capture(scanMs, pemMs, capMs);
            _debugManager.HandleCommand(command, pem);
            if (IsManualRescan(command)) _eventLogger.Record(_tickCount, "Manual rescan requested.");
            _displayManager.Render(_version, _tickCount, hardware, pem, capability, _performanceMonitor);
        }

        static bool IsManualRescan(string command)
        {
            return string.Equals(command, "scan", StringComparison.OrdinalIgnoreCase) || string.Equals(command, "rescan", StringComparison.OrdinalIgnoreCase);
        }
    }

    sealed class Configuration
    {
        public readonly string SystemTag;
        public readonly string FlightDisplayTag;
        public readonly string MaintenanceDisplayTag;
        public readonly string EngineeringDisplayTag;
        public Configuration(string systemTag)
        {
            SystemTag = systemTag;
            FlightDisplayTag = systemTag + " Flight";
            MaintenanceDisplayTag = systemTag + " Maintenance";
            EngineeringDisplayTag = systemTag + " Engineering";
        }
        public bool IsTagged(IMyTerminalBlock block) { return block != null && block.CustomName.IndexOf(SystemTag, StringComparison.OrdinalIgnoreCase) >= 0; }
    }

    sealed class HardwareDiscovery
    {
        readonly IMyGridTerminalSystem _gridTerminalSystem;
        readonly IMyProgrammableBlock _me;
        readonly Configuration _configuration;
        readonly List<IMyShipController> _controllers = new List<IMyShipController>();
        readonly List<IMyGravityGeneratorBase> _gravityGenerators = new List<IMyGravityGeneratorBase>();
        readonly List<IMyVirtualMass> _artificialMass = new List<IMyVirtualMass>();
        readonly List<IMyTextPanel> _textPanels = new List<IMyTextPanel>();
        readonly List<IMySoundBlock> _alarms = new List<IMySoundBlock>();
        readonly List<IMyLightingBlock> _warningLights = new List<IMyLightingBlock>();

        public HardwareDiscovery(IMyGridTerminalSystem gridTerminalSystem, IMyProgrammableBlock me, Configuration configuration)
        { _gridTerminalSystem = gridTerminalSystem; _me = me; _configuration = configuration; }

        public HardwareSnapshot Scan()
        {
            _controllers.Clear(); _gravityGenerators.Clear(); _artificialMass.Clear(); _textPanels.Clear(); _alarms.Clear(); _warningLights.Clear();
            _gridTerminalSystem.GetBlocksOfType(_controllers, SameConstruct);
            _gridTerminalSystem.GetBlocksOfType(_gravityGenerators, SameConstruct);
            _gridTerminalSystem.GetBlocksOfType(_artificialMass, SameConstruct);
            _gridTerminalSystem.GetBlocksOfType(_textPanels, SameConstruct);
            _gridTerminalSystem.GetBlocksOfType(_alarms, TaggedSameConstruct);
            _gridTerminalSystem.GetBlocksOfType(_warningLights, TaggedSameConstruct);
            IMyShipController controller = FirstUsableController();
            return new HardwareSnapshot(_controllers.Count, _gravityGenerators.Count, _artificialMass.Count, _textPanels.Count, _alarms.Count, _warningLights.Count, controller == null ? "none" : controller.CustomName, controller, _gravityGenerators, _artificialMass, CountTaggedDisplays(_configuration.FlightDisplayTag), CountTaggedDisplays(_configuration.MaintenanceDisplayTag), CountTaggedDisplays(_configuration.EngineeringDisplayTag), _configuration);
        }
        bool SameConstruct(IMyTerminalBlock block) { return block != null && block.IsSameConstructAs(_me); }
        bool TaggedSameConstruct(IMyTerminalBlock block) { return SameConstruct(block) && _configuration.IsTagged(block); }
        IMyShipController FirstUsableController()
        {
            for (int i = 0; i < _controllers.Count; i++) if (_controllers[i].IsMainCockpit || _controllers[i].CanControlShip) return _controllers[i];
            return _controllers.Count == 0 ? null : _controllers[0];
        }
        int CountTaggedDisplays(string tag)
        {
            int count = 0;
            for (int i = 0; i < _textPanels.Count; i++) if (_textPanels[i].CustomName.IndexOf(tag, StringComparison.OrdinalIgnoreCase) >= 0) count++;
            return count;
        }
    }

    sealed class HardwareSnapshot
    {
        public readonly int ControllerCount, GravityGeneratorCount, ArtificialMassCount, TextPanelCount, AlarmCount, WarningLightCount, FlightDisplayCount, MaintenanceDisplayCount, EngineeringDisplayCount;
        public readonly string PrimaryControllerName;
        public readonly IMyShipController PrimaryController;
        public readonly List<IMyGravityGeneratorBase> GravityGenerators;
        public readonly List<IMyVirtualMass> ArtificialMass;
        public readonly Configuration Configuration;
        public bool CoordinateFrameValid { get { return PrimaryController != null; } }
        public HardwareSnapshot(int controllerCount, int generatorCount, int massCount, int textPanelCount, int alarmCount, int warningLightCount, string primaryControllerName, IMyShipController primaryController, List<IMyGravityGeneratorBase> generators, List<IMyVirtualMass> mass, int flightDisplayCount, int maintenanceDisplayCount, int engineeringDisplayCount, Configuration configuration)
        {
            ControllerCount = controllerCount; GravityGeneratorCount = generatorCount; ArtificialMassCount = massCount; TextPanelCount = textPanelCount; AlarmCount = alarmCount; WarningLightCount = warningLightCount; PrimaryControllerName = primaryControllerName; PrimaryController = primaryController;
            GravityGenerators = new List<IMyGravityGeneratorBase>(generators); ArtificialMass = new List<IMyVirtualMass>(mass);
            FlightDisplayCount = flightDisplayCount; MaintenanceDisplayCount = maintenanceDisplayCount; EngineeringDisplayCount = engineeringDisplayCount; Configuration = configuration;
        }
    }

    sealed class PhysicsEngineModelBuilder
    {
        public PhysicsEngineModel Build(HardwareSnapshot hardware)
        {
            MatrixD frame = hardware.PrimaryController == null ? MatrixD.Identity : hardware.PrimaryController.WorldMatrix;
            List<PemBlock> generators = new List<PemBlock>();
            List<PemBlock> mass = new List<PemBlock>();
            for (int i = 0; i < hardware.GravityGenerators.Count; i++) generators.Add(PemBlock.FromBlock(hardware.GravityGenerators[i], hardware.Configuration.IsTagged(hardware.GravityGenerators[i]), hardware.CoordinateFrameValid, frame, true));
            for (int i = 0; i < hardware.ArtificialMass.Count; i++) mass.Add(PemBlock.FromBlock(hardware.ArtificialMass[i], hardware.Configuration.IsTagged(hardware.ArtificialMass[i]), hardware.CoordinateFrameValid, frame, false));
            return new PhysicsEngineModel(hardware.CoordinateFrameValid, hardware.PrimaryControllerName, generators, mass, hardware.GravityGeneratorCount, hardware.ArtificialMassCount);
        }
    }

    sealed class PemBlock
    {
        public readonly long EntityId; public readonly string StableId, CustomName, MountPosition, Orientation, GravityAxis, Validation, Kind, DistanceText;
        public readonly bool Tagged, Enabled, Working, Contributing; public readonly Vector3D WorldPosition; public readonly ShipAxis MountAxis, ProjectionAxis;
        public PemBlock(string kind, long entityId, string stableId, string customName, bool tagged, bool enabled, bool working, Vector3D worldPosition, ShipAxis mountAxis, ShipAxis projectionAxis, string mountPosition, string orientation, string gravityAxis, string validation, bool contributing, string distanceText)
        { Kind = kind; EntityId = entityId; StableId = stableId; CustomName = customName; Tagged = tagged; Enabled = enabled; Working = working; WorldPosition = worldPosition; MountAxis = mountAxis; ProjectionAxis = projectionAxis; MountPosition = mountPosition; Orientation = orientation; GravityAxis = gravityAxis; Validation = validation; Contributing = contributing; DistanceText = distanceText; }
        public static PemBlock FromBlock(IMyTerminalBlock block, bool tagged, bool frameValid, MatrixD frame, bool generator)
        {
            Vector3D pos = block.GetPosition();
            ShipAxis mount = frameValid ? AxisFromVector(pos - frame.Translation, frame) : ShipAxis.Forward;
            ShipAxis projection = frameValid ? AxisFromVector(block.WorldMatrix.Down, frame) : ShipAxis.Forward;
            bool enabled = true; IMyFunctionalBlock functional = block as IMyFunctionalBlock; if (functional != null) enabled = functional.Enabled;
            bool working = block.IsWorking;
            string validation = !frameValid ? "INVALID_FRAME" : !tagged ? "NOT_TAGGED" : !enabled ? "DISABLED" : !working ? "NOT_WORKING" : "READY";
            return new PemBlock(generator ? "GEN" : "MASS", block.EntityId, StableIdFor(block), block.CustomName, tagged, enabled, working, pos, mount, projection, AxisName(mount), OrientationName(block.WorldMatrix, frame, frameValid), AxisName(projection), validation, validation == "READY", DistanceFromFrame(pos, frame, frameValid));
        }
        static string DistanceFromFrame(Vector3D pos, MatrixD frame, bool valid) { return valid ? Vector3D.Distance(pos, frame.Translation).ToString("0.0") + " m" : "unknown"; }
        static string StableIdFor(IMyTerminalBlock block) { string id = Math.Abs(block.EntityId % 1000000).ToString("000000"); return block.GetType().Name + ":" + id; }
        static ShipAxis AxisFromVector(Vector3D vector, MatrixD frame)
        {
            if (vector.LengthSquared() < 0.0001) return ShipAxis.Forward;
            double best = Vector3D.Dot(vector, frame.Forward); ShipAxis axis = ShipAxis.Forward;
            TestAxis(vector, frame.Backward, ShipAxis.Backward, ref best, ref axis); TestAxis(vector, frame.Left, ShipAxis.Left, ref best, ref axis); TestAxis(vector, frame.Right, ShipAxis.Right, ref best, ref axis); TestAxis(vector, frame.Up, ShipAxis.Up, ref best, ref axis); TestAxis(vector, frame.Down, ShipAxis.Down, ref best, ref axis);
            return axis;
        }
        static void TestAxis(Vector3D vector, Vector3D candidate, ShipAxis candidateAxis, ref double best, ref ShipAxis axis) { double score = Vector3D.Dot(vector, candidate); if (score > best) { best = score; axis = candidateAxis; } }
        static string OrientationName(MatrixD block, MatrixD frame, bool valid) { if (!valid) return "unknown"; return "F:" + AxisName(AxisFromVector(block.Forward, frame)) + " U:" + AxisName(AxisFromVector(block.Up, frame)); }
        public static string AxisName(ShipAxis axis) { return axis == ShipAxis.Forward ? "Forward" : axis == ShipAxis.Backward ? "Backward" : axis == ShipAxis.Left ? "Left" : axis == ShipAxis.Right ? "Right" : axis == ShipAxis.Up ? "Up" : "Down"; }
    }

    sealed class PhysicsEngineModel
    {
        public readonly bool CoordinateFrameValid; public readonly string ReferenceControllerName; public readonly List<PemBlock> GravityGenerators, ArtificialMass; public readonly int DetectedGeneratorCount, DetectedMassCount;
        public readonly AxisCapability[] Axes = new AxisCapability[6]; public readonly string OverallHealth;
        public int TaggedGravityGeneratorCount { get { return CountTagged(GravityGenerators); } } public int TaggedArtificialMassCount { get { return CountTagged(ArtificialMass); } }
        public int ContributingGeneratorCount { get { return CountContributing(GravityGenerators); } } public int ContributingMassCount { get { return CountContributing(ArtificialMass); } }
        public int NonContributingCount { get { return GravityGenerators.Count + ArtificialMass.Count - ContributingGeneratorCount - ContributingMassCount; } }
        public bool Ready { get { for (int i = 0; i < Axes.Length; i++) if (Axes[i].Ready) return true; return false; } }
        public PhysicsEngineModel(bool coordinateFrameValid, string referenceControllerName, List<PemBlock> generators, List<PemBlock> mass, int detectedGeneratorCount, int detectedMassCount)
        {
            CoordinateFrameValid = coordinateFrameValid; ReferenceControllerName = referenceControllerName; GravityGenerators = generators; ArtificialMass = mass; DetectedGeneratorCount = detectedGeneratorCount; DetectedMassCount = detectedMassCount;
            for (int i = 0; i < 6; i++) Axes[i] = AxisCapability.Build((ShipAxis)i, generators, mass, coordinateFrameValid);
            OverallHealth = BuildHealth();
        }
        string BuildHealth() { if (!CoordinateFrameValid) return "INVALID FRAME"; if (ContributingGeneratorCount == 0 || ContributingMassCount == 0) return "NO PROPULSION"; return Ready ? "READY" : "LIMITED"; }
        static int CountTagged(List<PemBlock> blocks) { int c = 0; for (int i = 0; i < blocks.Count; i++) if (blocks[i].Tagged) c++; return c; }
        static int CountContributing(List<PemBlock> blocks) { int c = 0; for (int i = 0; i < blocks.Count; i++) if (blocks[i].Contributing) c++; return c; }
    }

    sealed class AxisCapability
    {
        public readonly ShipAxis Axis; public readonly int GeneratorCount, MassCount, Tolerance; public readonly bool Ready;
        public AxisCapability(ShipAxis axis, int generatorCount, int massCount, bool ready, int tolerance) { Axis = axis; GeneratorCount = generatorCount; MassCount = massCount; Ready = ready; Tolerance = tolerance; }
        public static AxisCapability Build(ShipAxis axis, List<PemBlock> generators, List<PemBlock> mass, bool frameValid)
        {
            int gen = CountForAxis(generators, axis); int am = CountForAxis(mass, axis); bool ready = frameValid && gen > 0; int tol = gen - 1; return new AxisCapability(axis, gen, am, ready, tol < 0 ? 0 : tol);
        }
        static int CountForAxis(List<PemBlock> blocks, ShipAxis axis) { int c = 0; for (int i = 0; i < blocks.Count; i++) if (blocks[i].Contributing && blocks[i].ProjectionAxis == axis) c++; return c; }
    }

    sealed class CapabilityAnalysis
    {
        public CapabilitySnapshot Evaluate(PhysicsEngineModel model)
        {
            int readyAxes = 0; for (int i = 0; i < model.Axes.Length; i++) if (model.Axes[i].Ready) readyAxes++;
            string status = model.Ready ? "OPERATIONAL" : "LIMITED";
            string message = model.Ready ? "READY axes " + readyAxes + "/6; monitor-only outputs locked" : "No READY axis; check frame, tags, enabled/working state, and generator/mass alignment";
            return new CapabilitySnapshot(status, message, model.Ready, readyAxes);
        }
    }
    sealed class CapabilitySnapshot { public readonly string Status, Message; public readonly bool PemReady; public readonly int ReadyAxisCount; public CapabilitySnapshot(string status, string message, bool pemReady, int readyAxisCount) { Status = status; Message = message; PemReady = pemReady; ReadyAxisCount = readyAxisCount; } }

    sealed class DebugManager
    {
        public const string SectionStatus = "status";
        public const string SectionDiscovery = "discovery";
        public const string SectionPem = "pem";
        public const string SectionCapability = "capability";
        public const string SectionGenerators = "generators";
        public const string SectionMass = "mass";
        public const string SectionPerformance = "performance";
        bool _enabled; string _section = SectionStatus; int _pageIndex;
        public bool Enabled { get { return _enabled; } } public string Section { get { return _section; } } public int PageIndex { get { return _pageIndex; } }
        public void HandleCommand(string command, PhysicsEngineModel pem)
        {
            if (string.IsNullOrWhiteSpace(command)) return; string n = command.Trim();
            if (Eq(n, "debug off")) { _enabled = false; _section = SectionStatus; _pageIndex = 0; return; }
            if (Eq(n, "debug disc")) { Enter(SectionDiscovery, 0); return; }
            if (Eq(n, "debug pem")) { Enter(SectionPem, 0); return; }
            if (Eq(n, "debug cap") || Eq(n, "debug capability")) { Enter(SectionCapability, 0); return; }
            if (Eq(n, "debug gen") || Eq(n, "debug generators")) { Enter(SectionGenerators, Clamp(_pageIndex, pem.GravityGenerators.Count)); return; }
            if (Eq(n, "debug mass")) { Enter(SectionMass, Clamp(_pageIndex, pem.ArtificialMass.Count)); return; }
            if (Eq(n, "debug perf")) { Enter(SectionPerformance, 0); return; }
            if (Eq(n, "debug next")) Next(pem);
            if (Eq(n, "debug prev")) Prev(pem);
        }
        void Enter(string section, int page) { _enabled = true; _section = section; _pageIndex = page; }
        void Next(PhysicsEngineModel pem) { if (!_enabled) return; int c = CountFor(pem); _pageIndex = c <= 1 ? 0 : (_pageIndex + 1) % c; }
        void Prev(PhysicsEngineModel pem) { if (!_enabled) return; int c = CountFor(pem); _pageIndex = c <= 1 ? 0 : (_pageIndex + c - 1) % c; }
        int CountFor(PhysicsEngineModel pem) { if (_section == SectionCapability) return 6; if (_section == SectionGenerators) return pem.GravityGenerators.Count; if (_section == SectionMass) return pem.ArtificialMass.Count; return 1; }
        int Clamp(int value, int count) { if (count <= 0) return 0; return value >= count ? count - 1 : value; }
        static bool Eq(string a, string b) { return string.Equals(a, b, StringComparison.OrdinalIgnoreCase); }
    }

    sealed class PerformanceMonitor
    {
        double _lastScanMs, _averageScanMs, _pemBuildMs, _capabilityMs; int _samples;
        public double LastScanMs { get { return _lastScanMs; } } public double AverageScanMs { get { return _averageScanMs; } } public double PemBuildMs { get { return _pemBuildMs; } } public double CapabilityMs { get { return _capabilityMs; } }
        public void Capture(double scanMs, double pemBuildMs, double capabilityMs) { _lastScanMs = scanMs; _pemBuildMs = pemBuildMs; _capabilityMs = capabilityMs; _samples++; _averageScanMs = _samples == 1 ? scanMs : ((_averageScanMs * (_samples - 1)) + scanMs) / _samples; }
    }

    sealed class DisplayManager
    {
        readonly IMyGridTerminalSystem _gridTerminalSystem; readonly IMyProgrammableBlock _me; readonly Configuration _configuration; readonly EventLogger _eventLogger; readonly DebugManager _debugManager; readonly List<IMyTextPanel> _panels = new List<IMyTextPanel>(); readonly StringBuilder _builder = new StringBuilder(); readonly Action<string> _echo;
        public DisplayManager(IMyGridTerminalSystem gridTerminalSystem, IMyProgrammableBlock me, Configuration configuration, EventLogger eventLogger, DebugManager debugManager, Action<string> echo) { _gridTerminalSystem = gridTerminalSystem; _me = me; _configuration = configuration; _eventLogger = eventLogger; _debugManager = debugManager; _echo = echo; }
        public void Render(string version, int tick, HardwareSnapshot hardware, PhysicsEngineModel pem, CapabilitySnapshot capability, PerformanceMonitor performance)
        {
            WriteSurface(_configuration.FlightDisplayTag, BuildFlightText(version, tick, hardware, pem));
            WriteSurface(_configuration.MaintenanceDisplayTag, BuildMaintenanceText(version, tick, hardware, pem));
            WriteSurface(_configuration.EngineeringDisplayTag, _debugManager.Enabled ? BuildConsoleDebugText(hardware, pem, capability, performance) : BuildConsoleStatusText(pem, capability));
            EchoSummary(version, tick, hardware, pem);
        }
        string BuildFlightText(string version, int tick, HardwareSnapshot hardware, PhysicsEngineModel pem) { _builder.Clear(); _builder.AppendLine("AAC FLIGHT"); _builder.AppendLine("v" + version + "  MONITOR ONLY"); _builder.AppendLine("POST : " + pem.OverallHealth); _builder.AppendLine("CTRL : " + ShortName(hardware.PrimaryControllerName, 20)); _builder.AppendLine("READY: " + ReadyAxes(pem) + "/6 axes"); _builder.AppendLine("GEN/MASS: " + pem.ContributingGeneratorCount + "/" + pem.ContributingMassCount); _builder.AppendLine("TICK : " + FormatTick(tick)); return _builder.ToString(); }
        string BuildMaintenanceText(string version, int tick, HardwareSnapshot hardware, PhysicsEngineModel pem) { _builder.Clear(); _builder.AppendLine("AAC MAINTENANCE"); _builder.AppendLine("v" + version + "  MONITOR ONLY"); _builder.AppendLine("Tick " + FormatTick(tick)); _builder.AppendLine("Health    : " + pem.OverallHealth); _builder.AppendLine("Controller: " + (hardware.CoordinateFrameValid ? "YES" : "NO")); _builder.AppendLine("Detected Gen/Mass: " + hardware.GravityGeneratorCount + "/" + hardware.ArtificialMassCount); _builder.AppendLine("Tagged   Gen/Mass: " + pem.TaggedGravityGeneratorCount + "/" + pem.TaggedArtificialMassCount); _builder.AppendLine("Contrib. Gen/Mass: " + pem.ContributingGeneratorCount + "/" + pem.ContributingMassCount); _builder.AppendLine("Non-Contributing : " + pem.NonContributingCount); AppendRedundancy(_builder, pem); _eventLogger.AppendTo(_builder); return _builder.ToString(); }
        string BuildConsoleStatusText(PhysicsEngineModel pem, CapabilitySnapshot cap) { _builder.Clear(); _builder.AppendLine("AAC CONSOLE"); _builder.AppendLine("Status"); _builder.AppendLine("Status : " + AttentionStatus(pem)); _builder.AppendLine("Overall: " + pem.OverallHealth); _builder.AppendLine("Axes READY: " + cap.ReadyAxisCount + "/6"); _builder.AppendLine("Warnings: " + WarningSummary(pem)); return _builder.ToString(); }
        string BuildConsoleDebugText(HardwareSnapshot hardware, PhysicsEngineModel pem, CapabilitySnapshot cap, PerformanceMonitor perf) { _builder.Clear(); AppendDebugHeader(_builder, pem); if (_debugManager.Section == DebugManager.SectionDiscovery) AppendDiscovery(_builder, pem); else if (_debugManager.Section == DebugManager.SectionPem) AppendPem(_builder, pem); else if (_debugManager.Section == DebugManager.SectionCapability) AppendCapability(_builder, pem); else if (_debugManager.Section == DebugManager.SectionGenerators) AppendGenerator(_builder, pem.GravityGenerators, _debugManager.PageIndex); else if (_debugManager.Section == DebugManager.SectionMass) AppendMass(_builder, pem.ArtificialMass, _debugManager.PageIndex); else if (_debugManager.Section == DebugManager.SectionPerformance) AppendPerformance(_builder, perf); else AppendConsoleStatusFields(_builder, pem, cap); return _builder.ToString(); }
        void AppendDebugHeader(StringBuilder b, PhysicsEngineModel pem) { b.AppendLine("AAC CONSOLE  DEBUG"); if (_debugManager.Section == DebugManager.SectionCapability) b.AppendLine("Capability (" + PemBlock.AxisName((ShipAxis)_debugManager.PageIndex) + ") " + (_debugManager.PageIndex + 1) + "/6"); else if (_debugManager.Section == DebugManager.SectionGenerators) b.AppendLine("Generator Inspector " + PageCount(_debugManager.PageIndex, pem.GravityGenerators.Count)); else if (_debugManager.Section == DebugManager.SectionMass) b.AppendLine("Mass Inspector " + PageCount(_debugManager.PageIndex, pem.ArtificialMass.Count)); else if (_debugManager.Section == DebugManager.SectionDiscovery) b.AppendLine("Discovery"); else if (_debugManager.Section == DebugManager.SectionPem) b.AppendLine("PEM"); else if (_debugManager.Section == DebugManager.SectionPerformance) b.AppendLine("Performance"); else b.AppendLine("Status"); }
        void AppendConsoleStatusFields(StringBuilder b, PhysicsEngineModel pem, CapabilitySnapshot cap) { b.AppendLine("Status : " + AttentionStatus(pem)); b.AppendLine("Overall: " + pem.OverallHealth); b.AppendLine("Axes READY: " + cap.ReadyAxisCount + "/6"); b.AppendLine("Warnings: " + WarningSummary(pem)); }
        void AppendDiscovery(StringBuilder b, PhysicsEngineModel p) { b.AppendLine("Gravity Generators: " + p.DetectedGeneratorCount + " / " + p.TaggedGravityGeneratorCount); b.AppendLine("Artificial Mass   : " + p.DetectedMassCount + " / " + p.TaggedArtificialMassCount); }
        void AppendPem(StringBuilder b, PhysicsEngineModel p) { b.AppendLine("Reference Controller: " + ShortName(p.ReferenceControllerName, 24)); b.AppendLine("Frame Validity      : " + (p.CoordinateFrameValid ? "VALID" : "INVALID")); b.AppendLine("Generator Contrib.  : " + p.ContributingGeneratorCount); b.AppendLine("Generator Non-Con.  : " + (p.GravityGenerators.Count - p.ContributingGeneratorCount)); b.AppendLine("Mass Contrib.       : " + p.ContributingMassCount); b.AppendLine("Mass Non-Con.       : " + (p.ArtificialMass.Count - p.ContributingMassCount)); }
        void AppendCapability(StringBuilder b, PhysicsEngineModel p) { AxisCapability a = p.Axes[_debugManager.PageIndex]; b.AppendLine("State     : " + (a.Ready ? "READY" : "NOT READY")); b.AppendLine("Reason    : " + CapabilityReason(p, a)); b.AppendLine("Generators: " + a.GeneratorCount); b.AppendLine("Tolerance : " + a.Tolerance); }
        void AppendGenerator(StringBuilder b, List<PemBlock> blocks, int index) { PemBlock x; if (!TryBlock(b, blocks, ref index, "No gravity generators.")) return; x = blocks[index]; b.AppendLine("Entity ID   : " + x.EntityId); b.AppendLine("Name        : " + ShortName(x.CustomName, 24)); b.AppendLine("Orientation : " + x.Orientation); b.AppendLine("Gravity Axis: " + x.GravityAxis); b.AppendLine("Distance    : " + x.DistanceText); b.AppendLine("Enabled     : " + YesNo(x.Enabled)); b.AppendLine("Working     : " + YesNo(x.Working)); b.AppendLine("Validated   : " + x.Validation); b.AppendLine("Contributing: " + YesNo(x.Contributing)); }
        void AppendMass(StringBuilder b, List<PemBlock> blocks, int index) { PemBlock x; if (!TryBlock(b, blocks, ref index, "No artificial mass blocks.")) return; x = blocks[index]; b.AppendLine("Entity ID   : " + x.EntityId); b.AppendLine("Name        : " + ShortName(x.CustomName, 24)); b.AppendLine("Distance    : " + x.DistanceText); b.AppendLine("Enabled     : " + YesNo(x.Enabled)); b.AppendLine("Working     : " + YesNo(x.Working)); b.AppendLine("Validated   : " + x.Validation); b.AppendLine("Contributing: " + YesNo(x.Contributing)); }
        void AppendPerformance(StringBuilder b, PerformanceMonitor p) { b.AppendLine("Last Scan                 : " + FormatMs(p.LastScanMs)); b.AppendLine("Average Scan              : " + FormatMs(p.AverageScanMs)); b.AppendLine("PEM Build Time            : " + FormatMs(p.PemBuildMs)); b.AppendLine("Capability Assessment Time: " + FormatMs(p.CapabilityMs)); }
        bool TryBlock(StringBuilder b, List<PemBlock> blocks, ref int index, string none) { if (blocks.Count == 0) { b.AppendLine(none); return false; } if (index >= blocks.Count) index = blocks.Count - 1; if (index < 0) index = 0; return true; }
        void AppendRedundancy(StringBuilder b, PhysicsEngineModel p) { b.AppendLine("Redundancy"); for (int i = 0; i < p.Axes.Length; i++) { AxisCapability a = p.Axes[i]; b.AppendLine("  " + PemBlock.AxisName(a.Axis) + " " + (a.Ready ? "READY" : "NOT READY") + " Gen " + a.GeneratorCount + " Tol " + a.Tolerance); } }
        string WarningSummary(PhysicsEngineModel p) { StringBuilder w = new StringBuilder(); if (!p.CoordinateFrameValid) AppendWarn(w, "PEM"); if (p.TaggedGravityGeneratorCount < p.DetectedGeneratorCount || p.ContributingGeneratorCount == 0) AppendWarn(w, "GEN"); if (p.TaggedArtificialMassCount < p.DetectedMassCount || p.ContributingMassCount == 0) AppendWarn(w, "MASS"); return w.Length == 0 ? "none" : "Hardware: " + w.ToString(); }
        static void AppendWarn(StringBuilder b, string value) { if (b.Length > 0) b.Append(" "); b.Append("(").Append(value).Append(")"); }
        static string AttentionStatus(PhysicsEngineModel p) { return p.Ready && p.NonContributingCount == 0 ? "OK" : "ATTENTION"; }
        static string CapabilityReason(PhysicsEngineModel p, AxisCapability a) { if (!p.CoordinateFrameValid) return "Invalid frame"; if (a.GeneratorCount == 0) return "No contributing generator"; return "Axis has contributing generator"; }
        int ReadyAxes(PhysicsEngineModel p) { int c = 0; for (int i = 0; i < p.Axes.Length; i++) if (p.Axes[i].Ready) c++; return c; }
        void EchoSummary(string version, int tick, HardwareSnapshot hardware, PhysicsEngineModel pem) { if (_echo == null) return; _builder.Clear(); _builder.AppendLine("AAC v" + version + " MONITOR ONLY"); _builder.AppendLine("Tick " + FormatTick(tick) + " " + pem.OverallHealth); _builder.AppendLine("Ctrl " + hardware.ControllerCount + " Gen " + hardware.GravityGeneratorCount + " Mass " + hardware.ArtificialMassCount); _builder.AppendLine(_debugManager.Enabled ? "Console DEBUG" : "Console Status"); _echo(_builder.ToString()); }
        static string PageCount(int index, int count) { return count == 0 ? "0/0" : (index + 1) + "/" + count; }
        static string FormatTick(int value) { return value.ToString("00000"); } static string YesNo(bool value) { return value ? "YES" : "NO"; } static string ShortName(string value, int max) { if (string.IsNullOrEmpty(value)) return "none"; return value.Length <= max ? value : value.Substring(0, max - 3) + "..."; } static string FormatMs(double value) { return value.ToString("0.000") + " ms"; }
        void WriteSurface(string tag, string text) { _panels.Clear(); _gridTerminalSystem.GetBlocksOfType(_panels, p => p.IsSameConstructAs(_me) && p.CustomName.IndexOf(tag, StringComparison.OrdinalIgnoreCase) >= 0); for (int i = 0; i < _panels.Count; i++) { _panels[i].ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE; _panels[i].Font = "Debug"; _panels[i].FontSize = 0.65f; _panels[i].WriteText(text, false); } }
    }

    sealed class EventLogger
    {
        readonly int _capacity; readonly Queue<string> _events = new Queue<string>();
        public EventLogger(int capacity) { _capacity = Math.Max(1, capacity); }
        public void Record(int tick, string message) { if (_events.Count >= _capacity) _events.Dequeue(); _events.Enqueue("[" + tick.ToString("00000") + "] " + message); }
        public void AppendTo(StringBuilder builder) { builder.AppendLine("Events:"); if (_events.Count == 0) { builder.AppendLine("- none"); return; } foreach (string item in _events) builder.AppendLine("- " + item); }
    }
