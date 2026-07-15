/*
===============================================================================
 Adaptive Antigravity Controller (AAC)
 Version: 0.3.0
 Project Lead: Nomaddison
 Development Assistance: OpenAI ChatGPT

 MILESTONE 3 PEM CAPABILITY ASSESSMENT
 Self-contained Space Engineers Programmable Block script.  Controller remains
 monitor-only: no propulsion, alarm, or lighting outputs are written.
===============================================================================
*/

    const string Version = "0.3.0";
    const string SystemTag = "[AAC]";

    readonly Configuration _configuration;
    readonly EventLogger _eventLogger;
    readonly HardwareDiscovery _hardwareDiscovery;
    readonly PhysicsEngineModelBuilder _pemBuilder;
    readonly CapabilityAnalysis _capabilityAnalysis;
    readonly DebugManager _debugManager;
    readonly DisplayManager _displayManager;
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
        _core = new AacCore(Version, _hardwareDiscovery, _pemBuilder, _capabilityAnalysis, _debugManager, _displayManager, _eventLogger);
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
        int _tickCount;

        public AacCore(string version, HardwareDiscovery hardwareDiscovery, PhysicsEngineModelBuilder pemBuilder, CapabilityAnalysis capabilityAnalysis, DebugManager debugManager, DisplayManager displayManager, EventLogger eventLogger)
        {
            _version = version;
            _hardwareDiscovery = hardwareDiscovery;
            _pemBuilder = pemBuilder;
            _capabilityAnalysis = capabilityAnalysis;
            _debugManager = debugManager;
            _displayManager = displayManager;
            _eventLogger = eventLogger;
        }

        public void Tick(string command)
        {
            _tickCount++;
            HardwareSnapshot hardware = _hardwareDiscovery.Scan();
            PhysicsEngineModel pem = _pemBuilder.Build(hardware);
            CapabilitySnapshot capability = _capabilityAnalysis.Evaluate(pem);
            _debugManager.HandleCommand(command, pem);
            if (IsManualRescan(command)) _eventLogger.Record(_tickCount, "Manual rescan requested.");
            _displayManager.Render(_version, _tickCount, hardware, pem, capability);
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
        public readonly long EntityId; public readonly string StableId, CustomName, MountPosition, Orientation, GravityAxis, Validation, Kind;
        public readonly bool Tagged, Enabled, Working, Contributing; public readonly Vector3D WorldPosition; public readonly ShipAxis MountAxis, ProjectionAxis;
        public PemBlock(string kind, long entityId, string stableId, string customName, bool tagged, bool enabled, bool working, Vector3D worldPosition, ShipAxis mountAxis, ShipAxis projectionAxis, string mountPosition, string orientation, string gravityAxis, string validation, bool contributing)
        { Kind = kind; EntityId = entityId; StableId = stableId; CustomName = customName; Tagged = tagged; Enabled = enabled; Working = working; WorldPosition = worldPosition; MountAxis = mountAxis; ProjectionAxis = projectionAxis; MountPosition = mountPosition; Orientation = orientation; GravityAxis = gravityAxis; Validation = validation; Contributing = contributing; }
        public static PemBlock FromBlock(IMyTerminalBlock block, bool tagged, bool frameValid, MatrixD frame, bool generator)
        {
            Vector3D pos = block.GetPosition();
            ShipAxis mount = frameValid ? AxisFromVector(pos - frame.Translation, frame) : ShipAxis.Forward;
            ShipAxis projection = frameValid ? AxisFromVector(block.WorldMatrix.Down, frame) : ShipAxis.Forward;
            bool enabled = true; IMyFunctionalBlock functional = block as IMyFunctionalBlock; if (functional != null) enabled = functional.Enabled;
            bool working = block.IsWorking;
            string validation = !frameValid ? "INVALID_FRAME" : !tagged ? "NOT_TAGGED" : !enabled ? "DISABLED" : !working ? "NOT_WORKING" : "READY";
            return new PemBlock(generator ? "GEN" : "MASS", block.EntityId, StableIdFor(block), block.CustomName, tagged, enabled, working, pos, mount, projection, AxisName(mount), OrientationName(block.WorldMatrix, frame, frameValid), AxisName(projection), validation, validation == "READY");
        }
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
            int gen = CountForAxis(generators, axis); int am = CountForAxis(mass, axis); bool ready = frameValid && gen > 0 && am > 0; int tol = ready ? Math.Min(gen, am) - 1 : 0; return new AxisCapability(axis, gen, am, ready, tol < 0 ? 0 : tol);
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
        readonly string[] _summaryPages = new string[] { "Overview", "Discovery", "PEM Summary", "Capability Analysis", "Performance" };
        bool _enabled; string _inspector = "summary"; int _pageIndex; int _itemIndex;
        public bool Enabled { get { return _enabled; } } public string Inspector { get { return _inspector; } } public int PageIndex { get { return _pageIndex; } } public int ItemIndex { get { return _itemIndex; } }
        public string ActivePageName { get { return _inspector == "generators" ? "Generator " + (_itemIndex + 1) : _inspector == "mass" ? "Mass " + (_itemIndex + 1) : _summaryPages[_pageIndex]; } }
        public void HandleCommand(string command, PhysicsEngineModel pem)
        {
            if (string.IsNullOrWhiteSpace(command)) return; string n = command.Trim();
            if (Eq(n, "debug on")) { _enabled = true; return; } if (Eq(n, "debug off")) { _enabled = false; return; }
            if (Eq(n, "debug generators") || Eq(n, "debug gen")) { _enabled = true; _inspector = "generators"; _itemIndex = Clamp(_itemIndex, CountFor(pem)); return; }
            if (Eq(n, "debug mass")) { _enabled = true; _inspector = "mass"; _itemIndex = Clamp(_itemIndex, CountFor(pem)); return; }
            if (Eq(n, "debug summary")) { _enabled = true; _inspector = "summary"; return; }
            if (Eq(n, "debug pem")) { _enabled = true; _inspector = "summary"; _pageIndex = 2; return; } if (Eq(n, "debug capability")) { _enabled = true; _inspector = "summary"; _pageIndex = 3; return; }
            if (Eq(n, "debug next")) Next(pem); if (Eq(n, "debug prev")) Prev(pem);
        }
        void Next(PhysicsEngineModel pem) { if (!_enabled) return; if (_inspector == "summary") _pageIndex = (_pageIndex + 1) % _summaryPages.Length; else _itemIndex = (CountFor(pem) == 0 ? 0 : (_itemIndex + 1) % CountFor(pem)); }
        void Prev(PhysicsEngineModel pem) { if (!_enabled) return; if (_inspector == "summary") _pageIndex = (_pageIndex + _summaryPages.Length - 1) % _summaryPages.Length; else { int c = CountFor(pem); _itemIndex = c == 0 ? 0 : (_itemIndex + c - 1) % c; } }
        int CountFor(PhysicsEngineModel pem) { return _inspector == "generators" ? pem.GravityGenerators.Count : _inspector == "mass" ? pem.ArtificialMass.Count : _summaryPages.Length; }
        int Clamp(int value, int count) { if (count <= 0) return 0; return value >= count ? count - 1 : value; }
        static bool Eq(string a, string b) { return string.Equals(a, b, StringComparison.OrdinalIgnoreCase); }
        public string StatusLine() { return !_enabled ? "Debug: OFF" : "Debug: " + ActivePageName; }
    }

    sealed class DisplayManager
    {
        readonly IMyGridTerminalSystem _gridTerminalSystem; readonly IMyProgrammableBlock _me; readonly Configuration _configuration; readonly EventLogger _eventLogger; readonly DebugManager _debugManager; readonly List<IMyTextPanel> _panels = new List<IMyTextPanel>(); readonly StringBuilder _builder = new StringBuilder(); readonly Action<string> _echo;
        public DisplayManager(IMyGridTerminalSystem gridTerminalSystem, IMyProgrammableBlock me, Configuration configuration, EventLogger eventLogger, DebugManager debugManager, Action<string> echo) { _gridTerminalSystem = gridTerminalSystem; _me = me; _configuration = configuration; _eventLogger = eventLogger; _debugManager = debugManager; _echo = echo; }
        public void Render(string version, int tick, HardwareSnapshot hardware, PhysicsEngineModel pem, CapabilitySnapshot capability)
        {
            WriteSurface(_configuration.FlightDisplayTag, BuildFlightText(version, tick, hardware, pem));
            WriteSurface(_configuration.MaintenanceDisplayTag, BuildMaintenanceText(version, tick, hardware, pem));
            WriteSurface(_configuration.EngineeringDisplayTag, _debugManager.Enabled ? BuildDebugText(version, tick, hardware, pem, capability) : BuildEngineeringText(version, tick, hardware, pem, capability));
            EchoSummary(version, tick, hardware, pem);
        }
        string BuildFlightText(string version, int tick, HardwareSnapshot hardware, PhysicsEngineModel pem) { _builder.Clear(); _builder.AppendLine("AAC FLIGHT"); _builder.AppendLine("v" + version + "  MONITOR ONLY"); _builder.AppendLine("POST : " + pem.OverallHealth); _builder.AppendLine("CTRL : " + ShortName(hardware.PrimaryControllerName, 20)); _builder.AppendLine("READY: " + ReadyAxes(pem) + "/6 axes"); _builder.AppendLine("GEN/MASS: " + pem.ContributingGeneratorCount + "/" + pem.ContributingMassCount); _builder.AppendLine("TICK : " + FormatTick(tick)); return _builder.ToString(); }
        string BuildMaintenanceText(string version, int tick, HardwareSnapshot hardware, PhysicsEngineModel pem) { _builder.Clear(); _builder.AppendLine("AAC MAINTENANCE"); _builder.AppendLine("v" + version + "  MONITOR ONLY"); _builder.AppendLine("Tick " + FormatTick(tick)); _builder.AppendLine("Health    : " + pem.OverallHealth); _builder.AppendLine("Controller: " + (hardware.CoordinateFrameValid ? "YES" : "NO")); _builder.AppendLine("Detected Gen/Mass: " + hardware.GravityGeneratorCount + "/" + hardware.ArtificialMassCount); _builder.AppendLine("Tagged   Gen/Mass: " + pem.TaggedGravityGeneratorCount + "/" + pem.TaggedArtificialMassCount); _builder.AppendLine("Contrib. Gen/Mass: " + pem.ContributingGeneratorCount + "/" + pem.ContributingMassCount); _builder.AppendLine("Non-Contributing : " + pem.NonContributingCount); AppendRedundancy(_builder, pem); _eventLogger.AppendTo(_builder); return _builder.ToString(); }
        string BuildEngineeringText(string version, int tick, HardwareSnapshot hardware, PhysicsEngineModel pem, CapabilitySnapshot cap) { _builder.Clear(); _builder.AppendLine("AAC ENGINEERING"); _builder.AppendLine("v" + version + "  MONITOR ONLY"); AppendPemSummary(_builder, pem); _builder.AppendLine("Capability: " + cap.Status); AppendWrapped(_builder, cap.Message, "  ", 30); return _builder.ToString(); }
        string BuildDebugText(string version, int tick, HardwareSnapshot hardware, PhysicsEngineModel pem, CapabilitySnapshot cap) { _builder.Clear(); _builder.AppendLine("AAC DEBUG"); _builder.AppendLine("v" + version + "  MONITOR ONLY"); _builder.AppendLine("Tick " + FormatTick(tick)); _builder.AppendLine(_debugManager.StatusLine()); _builder.AppendLine("----------------------------"); if (_debugManager.Inspector == "generators") AppendComponent(_builder, pem.GravityGenerators, _debugManager.ItemIndex); else if (_debugManager.Inspector == "mass") AppendComponent(_builder, pem.ArtificialMass, _debugManager.ItemIndex); else if (_debugManager.PageIndex == 0) AppendPemSummary(_builder, pem); else if (_debugManager.PageIndex == 1) AppendDiscovery(_builder, hardware); else if (_debugManager.PageIndex == 2) AppendPemSummary(_builder, pem); else if (_debugManager.PageIndex == 3) { _builder.AppendLine("Capability Analysis"); _builder.AppendLine("  Status: " + cap.Status); _builder.AppendLine("  READY axes: " + cap.ReadyAxisCount); AppendRedundancy(_builder, pem); } else { _builder.AppendLine("Performance"); _builder.AppendLine("  Cadence: Update100"); _builder.AppendLine("  Output : read-only"); } return _builder.ToString(); }
        void AppendDiscovery(StringBuilder b, HardwareSnapshot h) { b.AppendLine("Discovery"); b.AppendLine("  Controllers : " + h.ControllerCount); b.AppendLine("  Gravity Gen : " + h.GravityGeneratorCount); b.AppendLine("  Art. Mass   : " + h.ArtificialMassCount); b.AppendLine("  Text LCDs   : " + h.TextPanelCount); b.AppendLine("  Alarms/Lights: " + h.AlarmCount + "/" + h.WarningLightCount); }
        void AppendPemSummary(StringBuilder b, PhysicsEngineModel p) { b.AppendLine("PEM Summary"); b.AppendLine("  Health    : " + p.OverallHealth); b.AppendLine("  Reference : " + ShortName(p.ReferenceControllerName, 18)); b.AppendLine("  Frame     : " + (p.CoordinateFrameValid ? "VALID" : "INVALID")); b.AppendLine("  Detected  : " + p.DetectedGeneratorCount + " Gen / " + p.DetectedMassCount + " Mass"); b.AppendLine("  Tagged    : " + p.TaggedGravityGeneratorCount + " Gen / " + p.TaggedArtificialMassCount + " Mass"); b.AppendLine("  Contrib.  : " + p.ContributingGeneratorCount + " Gen / " + p.ContributingMassCount + " Mass"); b.AppendLine("  Non-Contrib: " + p.NonContributingCount); AppendRedundancy(b, p); }
        void AppendRedundancy(StringBuilder b, PhysicsEngineModel p) { b.AppendLine("Redundancy"); for (int i = 0; i < p.Axes.Length; i++) { AxisCapability a = p.Axes[i]; b.AppendLine("  " + PemBlock.AxisName(a.Axis) + " READY/" + (a.Ready ? "YES" : "NO") + " Gen " + a.GeneratorCount + " Tol " + a.Tolerance); } }
        void AppendComponent(StringBuilder b, List<PemBlock> blocks, int index) { if (blocks.Count == 0) { b.AppendLine("No components in inspector."); return; } if (index >= blocks.Count) index = blocks.Count - 1; PemBlock x = blocks[index]; b.AppendLine(x.Kind + " Component " + (index + 1) + "/" + blocks.Count); b.AppendLine("  ID     : " + x.StableId); b.AppendLine("  Name   : " + ShortName(x.CustomName, 26)); b.AppendLine("  Tagged : " + YesNo(x.Tagged)); b.AppendLine("  Enabled: " + YesNo(x.Enabled)); b.AppendLine("  Working: " + YesNo(x.Working)); b.AppendLine("  State  : " + x.Validation); b.AppendLine("  Contrib: " + YesNo(x.Contributing)); b.AppendLine("  Mount  : " + x.MountPosition); b.AppendLine("  Orient : " + x.Orientation); b.AppendLine("  GravAx : " + x.GravityAxis); }
        int ReadyAxes(PhysicsEngineModel p) { int c = 0; for (int i = 0; i < p.Axes.Length; i++) if (p.Axes[i].Ready) c++; return c; }
        void EchoSummary(string version, int tick, HardwareSnapshot hardware, PhysicsEngineModel pem) { if (_echo == null) return; _builder.Clear(); _builder.AppendLine("AAC v" + version + " MONITOR ONLY"); _builder.AppendLine("Tick " + FormatTick(tick) + " " + pem.OverallHealth); _builder.AppendLine("Ctrl " + hardware.ControllerCount + " Gen " + hardware.GravityGeneratorCount + " Mass " + hardware.ArtificialMassCount); _builder.AppendLine(_debugManager.StatusLine()); _echo(_builder.ToString()); }
        static string FormatTick(int value) { return value.ToString("00000"); } static string YesNo(bool value) { return value ? "YES" : "NO"; } static string ShortName(string value, int max) { if (string.IsNullOrEmpty(value)) return "none"; return value.Length <= max ? value : value.Substring(0, max - 3) + "..."; }
        static void AppendWrapped(StringBuilder b, string text, string indent, int width) { if (string.IsNullOrEmpty(text)) { b.AppendLine(indent + "none"); return; } int i = 0; while (i < text.Length) { int take = Math.Min(width, text.Length - i); b.AppendLine(indent + text.Substring(i, take)); i += take; } }
        void WriteSurface(string tag, string text) { _panels.Clear(); _gridTerminalSystem.GetBlocksOfType(_panels, p => p.IsSameConstructAs(_me) && p.CustomName.IndexOf(tag, StringComparison.OrdinalIgnoreCase) >= 0); for (int i = 0; i < _panels.Count; i++) { _panels[i].ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE; _panels[i].Font = "Debug"; _panels[i].FontSize = 0.65f; _panels[i].WriteText(text, false); } }
    }

    sealed class EventLogger
    {
        readonly int _capacity; readonly Queue<string> _events = new Queue<string>();
        public EventLogger(int capacity) { _capacity = Math.Max(1, capacity); }
        public void Record(int tick, string message) { if (_events.Count >= _capacity) _events.Dequeue(); _events.Enqueue("[" + tick.ToString("00000") + "] " + message); }
        public void AppendTo(StringBuilder builder) { builder.AppendLine("Events:"); if (_events.Count == 0) { builder.AppendLine("- none"); return; } foreach (string item in _events) builder.AppendLine("- " + item); }
    }
