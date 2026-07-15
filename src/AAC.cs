/*
===============================================================================
 Adaptive Antigravity Controller (AAC)
 Version: 0.3.0-alpha.1
 Project Lead: Nomaddison
 Development Assistance: OpenAI ChatGPT

 MILESTONE 3 PHYSICS ENGINE MODEL ENHANCEMENT
 This script makes the PEM the authoritative monitor-only digital twin for
 AAC-owned propulsion hardware, including derived orientation, gravity axis,
 lifecycle validation, dynamic capability groups, and read-only inspectors.
===============================================================================
*/

    const string Version = "0.3.0-alpha.1";
    const string SystemTag = "[AAC]";

    readonly Configuration _configuration;
    readonly EventLogger _eventLogger;
    readonly HardwareDiscovery _hardwareDiscovery;
    readonly Diagnostics _diagnostics;
    readonly PhysicsEngineModelBuilder _physicsEngineModelBuilder;
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
        _diagnostics = new Diagnostics();
        _physicsEngineModelBuilder = new PhysicsEngineModelBuilder();
        _capabilityAnalysis = new CapabilityAnalysis();
        _debugManager = new DebugManager();
        _displayManager = new DisplayManager(GridTerminalSystem, Me, _configuration, _eventLogger, _debugManager, Echo);
        _core = new AacCore(Version, _hardwareDiscovery, _diagnostics, _physicsEngineModelBuilder, _capabilityAnalysis, _debugManager, _displayManager, _eventLogger);

        _eventLogger.Record(0, "AAC boot: monitor-only PEM digital twin online.");
        _core.Tick("boot");
    }

    public void Main(string argument, UpdateType updateSource)
    {
        _core.Tick(argument == null ? string.Empty : argument.Trim());
    }

    sealed class AacCore
    {
        readonly string _version;
        readonly HardwareDiscovery _hardwareDiscovery;
        readonly Diagnostics _diagnostics;
        readonly PhysicsEngineModelBuilder _physicsEngineModelBuilder;
        readonly CapabilityAnalysis _capabilityAnalysis;
        readonly DebugManager _debugManager;
        readonly DisplayManager _displayManager;
        readonly EventLogger _eventLogger;
        int _tickCount;

        public AacCore(
            string version,
            HardwareDiscovery hardwareDiscovery,
            Diagnostics diagnostics,
            PhysicsEngineModelBuilder physicsEngineModelBuilder,
            CapabilityAnalysis capabilityAnalysis,
            DebugManager debugManager,
            DisplayManager displayManager,
            EventLogger eventLogger)
        {
            _version = version;
            _hardwareDiscovery = hardwareDiscovery;
            _diagnostics = diagnostics;
            _physicsEngineModelBuilder = physicsEngineModelBuilder;
            _capabilityAnalysis = capabilityAnalysis;
            _debugManager = debugManager;
            _displayManager = displayManager;
            _eventLogger = eventLogger;
        }

        public void Tick(string command)
        {
            _tickCount++;

            HardwareSnapshot hardware = _hardwareDiscovery.Scan();
            DiagnosticSnapshot diagnostic = _diagnostics.Evaluate(hardware);
            PhysicsEngineModel physicsEngineModel = _physicsEngineModelBuilder.Build(hardware);
            CapabilitySnapshot capability = _capabilityAnalysis.Evaluate(physicsEngineModel);

            _debugManager.HandleCommand(command);

            if (IsManualRescan(command))
                _eventLogger.Record(_tickCount, "Manual rescan requested.");

            _displayManager.Render(_version, _tickCount, hardware, diagnostic, physicsEngineModel, capability);
        }

        static bool IsManualRescan(string command)
        {
            return string.Equals(command, "scan", StringComparison.OrdinalIgnoreCase)
                || string.Equals(command, "rescan", StringComparison.OrdinalIgnoreCase);
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

        public bool IsTagged(IMyTerminalBlock block)
        {
            return block != null && block.CustomName.IndexOf(SystemTag, StringComparison.OrdinalIgnoreCase) >= 0;
        }
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

        public HardwareDiscovery(
            IMyGridTerminalSystem gridTerminalSystem,
            IMyProgrammableBlock me,
            Configuration configuration)
        {
            _gridTerminalSystem = gridTerminalSystem;
            _me = me;
            _configuration = configuration;
        }

        public HardwareSnapshot Scan()
        {
            _controllers.Clear();
            _gravityGenerators.Clear();
            _artificialMass.Clear();
            _textPanels.Clear();
            _alarms.Clear();
            _warningLights.Clear();

            _gridTerminalSystem.GetBlocksOfType(_controllers, SameConstruct);
            _gridTerminalSystem.GetBlocksOfType(_gravityGenerators, SameConstruct);
            _gridTerminalSystem.GetBlocksOfType(_artificialMass, SameConstruct);
            _gridTerminalSystem.GetBlocksOfType(_textPanels, SameConstruct);
            _gridTerminalSystem.GetBlocksOfType(_alarms, TaggedSameConstruct);
            _gridTerminalSystem.GetBlocksOfType(_warningLights, TaggedSameConstruct);

            IMyShipController primaryController = FirstUsableController();
            List<HardwareBlockMetadata> taggedGenerators = BuildTaggedMetadata(_gravityGenerators, primaryController);
            List<HardwareBlockMetadata> taggedMass = BuildTaggedMetadata(_artificialMass, primaryController);

            return new HardwareSnapshot(
                _controllers.Count,
                _gravityGenerators.Count,
                _artificialMass.Count,
                _textPanels.Count,
                _alarms.Count,
                _warningLights.Count,
                PrimaryControllerName(primaryController),
                primaryController,
                taggedGenerators,
                taggedMass,
                CountTaggedDisplays(_configuration.FlightDisplayTag),
                CountTaggedDisplays(_configuration.MaintenanceDisplayTag),
                CountTaggedDisplays(_configuration.EngineeringDisplayTag));
        }

        bool SameConstruct(IMyTerminalBlock block)
        {
            return block != null && block.IsSameConstructAs(_me);
        }

        bool TaggedSameConstruct(IMyTerminalBlock block)
        {
            return SameConstruct(block) && _configuration.IsTagged(block);
        }

        IMyShipController FirstUsableController()
        {
            for (int i = 0; i < _controllers.Count; i++)
            {
                if (_controllers[i].IsMainCockpit || _controllers[i].CanControlShip)
                    return _controllers[i];
            }

            return _controllers.Count == 0 ? null : _controllers[0];
        }

        string PrimaryControllerName(IMyShipController controller)
        {
            return controller == null ? "none" : controller.CustomName;
        }

        List<HardwareBlockMetadata> BuildTaggedMetadata<T>(List<T> blocks, IMyShipController controller) where T : class, IMyTerminalBlock
        {
            List<HardwareBlockMetadata> metadata = new List<HardwareBlockMetadata>();
            for (int i = 0; i < blocks.Count; i++)
            {
                if (_configuration.IsTagged(blocks[i]))
                    metadata.Add(HardwareBlockMetadata.FromBlock(blocks[i], controller));
            }
            return metadata;
        }

        int CountTaggedDisplays(string tag)
        {
            int count = 0;
            for (int i = 0; i < _textPanels.Count; i++)
            {
                if (_textPanels[i].CustomName.IndexOf(tag, StringComparison.OrdinalIgnoreCase) >= 0)
                    count++;
            }
            return count;
        }
    }

    sealed class HardwareBlockMetadata
    {
        public readonly long EntityId;
        public readonly string CustomName;
        public readonly string MountPosition;
        public readonly string BlockOrientation;
        public readonly string GravityProjectionAxis;
        public readonly double DistanceFromController;
        public readonly bool Enabled;
        public readonly bool Working;
        public readonly bool Validated;
        public readonly bool Contributing;
        public readonly string DebugId;

        public HardwareBlockMetadata(long entityId, string customName, string mountPosition, string blockOrientation, string gravityProjectionAxis, double distanceFromController, bool enabled, bool working)
        {
            EntityId = entityId;
            CustomName = customName;
            MountPosition = mountPosition;
            BlockOrientation = blockOrientation;
            GravityProjectionAxis = gravityProjectionAxis;
            DistanceFromController = distanceFromController;
            Enabled = enabled;
            Working = working;
            Validated = mountPosition != "unknown" && blockOrientation != "unknown" && gravityProjectionAxis != "unknown" && working;
            Contributing = Validated && enabled;
            DebugId = "UNASSIGNED";
        }

        HardwareBlockMetadata(HardwareBlockMetadata source, string debugId)
        {
            EntityId = source.EntityId;
            CustomName = source.CustomName;
            MountPosition = source.MountPosition;
            BlockOrientation = source.BlockOrientation;
            GravityProjectionAxis = source.GravityProjectionAxis;
            DistanceFromController = source.DistanceFromController;
            Enabled = source.Enabled;
            Working = source.Working;
            Validated = source.Validated;
            Contributing = source.Contributing;
            DebugId = debugId;
        }

        public HardwareBlockMetadata WithDebugId(string prefix, int index)
        {
            return new HardwareBlockMetadata(this, prefix + "-" + index.ToString("000"));
        }

        public static HardwareBlockMetadata FromBlock(IMyTerminalBlock block, IMyShipController controller)
        {
            string mountPosition = "unknown";
            string blockOrientation = "unknown";
            string gravityProjectionAxis = "unknown";
            double distance = 0.0;
            bool enabled = false;
            bool working = false;

            if (block != null)
            {
                IMyFunctionalBlock functional = block as IMyFunctionalBlock;
                enabled = functional == null || functional.Enabled;
                working = block.IsWorking;
            }

            if (block != null && controller != null)
            {
                Vector3D offset = block.GetPosition() - controller.GetPosition();
                distance = offset.Length();
                mountPosition = DirectionName(offset, controller.WorldMatrix);
                blockOrientation = AxisName(block.WorldMatrix.Forward, controller.WorldMatrix);
                gravityProjectionAxis = AxisName(block.WorldMatrix.Down, controller.WorldMatrix);
            }

            return new HardwareBlockMetadata(
                block == null ? 0 : block.EntityId,
                block == null ? "unknown" : block.CustomName,
                mountPosition,
                blockOrientation,
                gravityProjectionAxis,
                distance,
                enabled,
                working);
        }

        static string DirectionName(Vector3D offset, MatrixD reference)
        {
            if (offset.LengthSquared() < 0.0001)
                return "Center";
            return AxisName(offset, reference);
        }

        static string AxisName(Vector3D vector, MatrixD reference)
        {
            if (vector.LengthSquared() < 0.0001)
                return "unknown";

            double forward = Vector3D.Dot(vector, reference.Forward);
            double backward = Vector3D.Dot(vector, reference.Backward);
            double left = Vector3D.Dot(vector, reference.Left);
            double right = Vector3D.Dot(vector, reference.Right);
            double up = Vector3D.Dot(vector, reference.Up);
            double down = Vector3D.Dot(vector, reference.Down);

            double best = forward;
            string name = "Forward";
            if (backward > best) { best = backward; name = "Backward"; }
            if (left > best) { best = left; name = "Left"; }
            if (right > best) { best = right; name = "Right"; }
            if (up > best) { best = up; name = "Up"; }
            if (down > best) { name = "Down"; }
            return name;
        }
    }

    sealed class HardwareSnapshot
    {
        public readonly int ControllerCount;
        public readonly int GravityGeneratorCount;
        public readonly int ArtificialMassCount;
        public readonly int TextPanelCount;
        public readonly int AlarmCount;
        public readonly int WarningLightCount;
        public readonly string PrimaryControllerName;
        public readonly IMyShipController PrimaryController;
        public readonly List<HardwareBlockMetadata> TaggedGravityGenerators;
        public readonly List<HardwareBlockMetadata> TaggedArtificialMass;
        public readonly int FlightDisplayCount;
        public readonly int MaintenanceDisplayCount;
        public readonly int EngineeringDisplayCount;

        public int TaggedGravityGeneratorCount { get { return TaggedGravityGenerators.Count; } }
        public int TaggedArtificialMassCount { get { return TaggedArtificialMass.Count; } }
        public bool CoordinateFrameValid { get { return PrimaryController != null; } }

        public HardwareSnapshot(
            int controllerCount,
            int gravityGeneratorCount,
            int artificialMassCount,
            int textPanelCount,
            int alarmCount,
            int warningLightCount,
            string primaryControllerName,
            IMyShipController primaryController,
            List<HardwareBlockMetadata> taggedGravityGenerators,
            List<HardwareBlockMetadata> taggedArtificialMass,
            int flightDisplayCount,
            int maintenanceDisplayCount,
            int engineeringDisplayCount)
        {
            ControllerCount = controllerCount;
            GravityGeneratorCount = gravityGeneratorCount;
            ArtificialMassCount = artificialMassCount;
            TextPanelCount = textPanelCount;
            AlarmCount = alarmCount;
            WarningLightCount = warningLightCount;
            PrimaryControllerName = primaryControllerName;
            PrimaryController = primaryController;
            TaggedGravityGenerators = taggedGravityGenerators;
            TaggedArtificialMass = taggedArtificialMass;
            FlightDisplayCount = flightDisplayCount;
            MaintenanceDisplayCount = maintenanceDisplayCount;
            EngineeringDisplayCount = engineeringDisplayCount;
        }
    }

    sealed class Diagnostics
    {
        public DiagnosticSnapshot Evaluate(HardwareSnapshot hardware)
        {
            bool hasController = hardware.ControllerCount > 0;
            bool hasGravityDrivePair = hardware.GravityGeneratorCount > 0
                && hardware.ArtificialMassCount > 0;
            bool hasDisplay = hardware.FlightDisplayCount
                + hardware.MaintenanceDisplayCount
                + hardware.EngineeringDisplayCount > 0;

            List<string> findings = new List<string>();
            if (!hasController)
                findings.Add("missing ship controller");
            if (hardware.GravityGeneratorCount == 0)
                findings.Add("missing gravity generator");
            if (hardware.ArtificialMassCount == 0)
                findings.Add("missing artificial mass");
            if (!hasDisplay)
                findings.Add("no tagged AAC LCD");

            string level = hasController && hasGravityDrivePair ? "READY" : "INCOMPLETE";
            string message = findings.Count == 0 ? "Hardware Scan Complete" : JoinFindings(findings);

            return new DiagnosticSnapshot(level, message, hasController, hasGravityDrivePair, hasDisplay);
        }

        static string JoinFindings(List<string> findings)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < findings.Count; i++)
            {
                if (i > 0)
                    builder.Append("; ");
                builder.Append(findings[i]);
            }
            return builder.ToString();
        }
    }

    sealed class DiagnosticSnapshot
    {
        public readonly string Level;
        public readonly string Message;
        public readonly bool HasController;
        public readonly bool HasGravityDrivePair;
        public readonly bool HasDisplay;

        public DiagnosticSnapshot(
            string level,
            string message,
            bool hasController,
            bool hasGravityDrivePair,
            bool hasDisplay)
        {
            Level = level;
            Message = message;
            HasController = hasController;
            HasGravityDrivePair = hasGravityDrivePair;
            HasDisplay = hasDisplay;
        }
    }

    sealed class PhysicsEngineModel
    {
        public readonly bool CoordinateFrameValid;
        public readonly string ReferenceControllerName;
        public readonly List<HardwareBlockMetadata> GravityGenerators;
        public readonly List<HardwareBlockMetadata> ArtificialMass;

        public int TaggedGravityGeneratorCount { get { return GravityGenerators.Count; } }
        public int TaggedArtificialMassCount { get { return ArtificialMass.Count; } }
        public int ContributingGeneratorCount { get { return CountContributing(GravityGenerators); } }
        public int ContributingMassCount { get { return CountContributing(ArtificialMass); } }
        public int NonContributingGeneratorCount { get { return TaggedGravityGeneratorCount - ContributingGeneratorCount; } }
        public int NonContributingMassCount { get { return TaggedArtificialMassCount - ContributingMassCount; } }
        public bool HasTaggedDrivePair { get { return TaggedGravityGeneratorCount > 0 && TaggedArtificialMassCount > 0; } }
        public bool Ready { get { return CoordinateFrameValid && ContributingGeneratorCount > 0 && ContributingMassCount > 0; } }

        public PhysicsEngineModel(
            bool coordinateFrameValid,
            string referenceControllerName,
            List<HardwareBlockMetadata> gravityGenerators,
            List<HardwareBlockMetadata> artificialMass)
        {
            CoordinateFrameValid = coordinateFrameValid;
            ReferenceControllerName = referenceControllerName;
            GravityGenerators = AssignDebugIds(gravityGenerators, "GEN");
            ArtificialMass = AssignDebugIds(artificialMass, "MASS");
        }

        static List<HardwareBlockMetadata> AssignDebugIds(List<HardwareBlockMetadata> source, string prefix)
        {
            List<HardwareBlockMetadata> sorted = new List<HardwareBlockMetadata>(source);
            sorted.Sort((a, b) => a.EntityId.CompareTo(b.EntityId));
            for (int i = 0; i < sorted.Count; i++)
                sorted[i] = sorted[i].WithDebugId(prefix, i + 1);
            return sorted;
        }

        static int CountContributing(List<HardwareBlockMetadata> blocks)
        {
            int count = 0;
            for (int i = 0; i < blocks.Count; i++)
                if (blocks[i].Contributing)
                    count++;
            return count;
        }
    }

    sealed class CapabilityAnalysis
    {
        static readonly string[] Axes = new string[] { "Forward", "Backward", "Left", "Right", "Up", "Down" };

        public CapabilitySnapshot Evaluate(PhysicsEngineModel model)
        {
            List<AxisCapability> axes = new List<AxisCapability>();
            for (int i = 0; i < Axes.Length; i++)
                axes.Add(EvaluateAxis(Axes[i], model));

            List<string> findings = new List<string>();
            if (!model.CoordinateFrameValid)
                findings.Add("invalid coordinate frame");
            if (model.TaggedGravityGeneratorCount == 0)
                findings.Add("no AAC-tagged generators");
            if (model.TaggedArtificialMassCount == 0)
                findings.Add("no AAC-tagged artificial mass");
            for (int i = 0; i < axes.Count; i++)
                if (!axes[i].Ready)
                    findings.Add(axes[i].Axis + ": " + axes[i].Reason);

            string status = findings.Count == 0 ? "OPERATIONAL" : "LIMITED";
            string message = findings.Count == 0
                ? "dynamic PEM capability groups ready for monitor-only validation"
                : JoinFindings(findings);

            return new CapabilitySnapshot(status, message, model.Ready, axes);
        }

        AxisCapability EvaluateAxis(string axis, PhysicsEngineModel model)
        {
            int generatorCount = 0;
            int toleranceCount = 0;
            int invalidForAxis = 0;

            for (int i = 0; i < model.GravityGenerators.Count; i++)
            {
                HardwareBlockMetadata generator = model.GravityGenerators[i];
                if (string.Equals(generator.GravityProjectionAxis, axis, StringComparison.OrdinalIgnoreCase))
                {
                    if (generator.Contributing)
                        generatorCount++;
                    else
                        invalidForAxis++;
                }
            }

            toleranceCount = generatorCount > 0 ? generatorCount - 1 : 0;

            string reason = "READY";
            bool ready = true;
            if (!model.CoordinateFrameValid)
            {
                ready = false;
                reason = "Invalid coordinate frame";
            }
            else if (model.ContributingMassCount == 0)
            {
                ready = false;
                reason = "No contributing artificial mass";
            }
            else if (generatorCount == 0)
            {
                ready = false;
                reason = invalidForAxis > 0 ? "Generators not validated" : "No contributing generators";
            }

            return new AxisCapability(axis, ready, generatorCount, toleranceCount, reason);
        }

        static string JoinFindings(List<string> findings)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < findings.Count; i++)
            {
                if (i > 0)
                    builder.Append("; ");
                builder.Append(findings[i]);
            }
            return builder.ToString();
        }
    }

    sealed class AxisCapability
    {
        public readonly string Axis;
        public readonly bool Ready;
        public readonly int GeneratorCount;
        public readonly int ToleranceCount;
        public readonly string Reason;

        public AxisCapability(string axis, bool ready, int generatorCount, int toleranceCount, string reason)
        {
            Axis = axis;
            Ready = ready;
            GeneratorCount = generatorCount;
            ToleranceCount = toleranceCount;
            Reason = reason;
        }
    }

    sealed class CapabilitySnapshot
    {
        public readonly string Status;
        public readonly string Message;
        public readonly bool PemReady;
        public readonly List<AxisCapability> Axes;

        public CapabilitySnapshot(string status, string message, bool pemReady, List<AxisCapability> axes)
        {
            Status = status;
            Message = message;
            PemReady = pemReady;
            Axes = axes;
        }
    }


    sealed class DebugManager
    {
        readonly string[] _pages = new string[]
        {
            "Overview",
            "Discovery",
            "PEM Summary",
            "Capability Analysis",
            "Performance Placeholder"
        };
        bool _enabled;
        int _pageIndex;
        string _inspector = "";
        int _inspectorIndex;

        public bool Enabled { get { return _enabled; } }
        public int PageIndex { get { return _pageIndex; } }
        public int PageCount { get { return _pages.Length; } }
        public bool InspectingGenerators { get { return string.Equals(_inspector, "generators", StringComparison.OrdinalIgnoreCase); } }
        public bool InspectingMass { get { return string.Equals(_inspector, "mass", StringComparison.OrdinalIgnoreCase); } }
        public int InspectorIndex { get { return _inspectorIndex; } }
        public string ActivePageName
        {
            get
            {
                if (InspectingGenerators)
                    return "Generator Inspector";
                if (InspectingMass)
                    return "Artificial Mass Inspector";
                return _pages[_pageIndex];
            }
        }

        public void HandleCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;

            string normalized = command.Trim();
            if (string.Equals(normalized, "debug on", StringComparison.OrdinalIgnoreCase)) { _enabled = true; return; }
            if (string.Equals(normalized, "debug off", StringComparison.OrdinalIgnoreCase)) { _enabled = false; _inspector = ""; return; }
            if (string.Equals(normalized, "debug generators", StringComparison.OrdinalIgnoreCase)) { _enabled = true; _inspector = "generators"; _inspectorIndex = 0; return; }
            if (string.Equals(normalized, "debug mass", StringComparison.OrdinalIgnoreCase)) { _enabled = true; _inspector = "mass"; _inspectorIndex = 0; return; }
            if (string.Equals(normalized, "debug next", StringComparison.OrdinalIgnoreCase))
            {
                if (_enabled && _inspector.Length > 0) _inspectorIndex++;
                else if (_enabled) _pageIndex = (_pageIndex + 1) % _pages.Length;
                return;
            }
            if (string.Equals(normalized, "debug prev", StringComparison.OrdinalIgnoreCase))
            {
                if (_enabled && _inspector.Length > 0) _inspectorIndex = Math.Max(0, _inspectorIndex - 1);
                else if (_enabled) _pageIndex = (_pageIndex + _pages.Length - 1) % _pages.Length;
                return;
            }
            if (string.Equals(normalized, "debug discovery", StringComparison.OrdinalIgnoreCase)) { _enabled = true; _inspector = ""; _pageIndex = 1; return; }
            if (string.Equals(normalized, "debug pem", StringComparison.OrdinalIgnoreCase)) { _enabled = true; _inspector = ""; _pageIndex = 2; return; }
            if (string.Equals(normalized, "debug capability", StringComparison.OrdinalIgnoreCase)) { _enabled = true; _inspector = ""; _pageIndex = 3; return; }
            if (string.Equals(normalized, "debug performance", StringComparison.OrdinalIgnoreCase)) { _enabled = true; _inspector = ""; _pageIndex = 4; }
        }

        public int ActiveOrdinal(int itemCount)
        {
            if (itemCount <= 0)
                return 0;
            if (_inspectorIndex >= itemCount)
                _inspectorIndex = itemCount - 1;
            return _inspectorIndex;
        }

        public string StatusLine()
        {
            if (!_enabled)
                return "Debug: OFF";
            if (_inspector.Length > 0)
                return "Debug: " + ActivePageName + " (Item " + (_inspectorIndex + 1) + ")";
            return "Debug: " + ActivePageName + " (Page " + (_pageIndex + 1) + "/" + _pages.Length + ")";
        }
    }

    sealed class DisplayManager
    {
        readonly IMyGridTerminalSystem _gridTerminalSystem;
        readonly IMyProgrammableBlock _me;
        readonly Configuration _configuration;
        readonly EventLogger _eventLogger;
        readonly DebugManager _debugManager;
        readonly List<IMyTextPanel> _panels = new List<IMyTextPanel>();
        readonly StringBuilder _builder = new StringBuilder();
        readonly Action<string> _echo;

        public DisplayManager(
            IMyGridTerminalSystem gridTerminalSystem,
            IMyProgrammableBlock me,
            Configuration configuration,
            EventLogger eventLogger,
            DebugManager debugManager,
            Action<string> echo)
        {
            _gridTerminalSystem = gridTerminalSystem;
            _me = me;
            _configuration = configuration;
            _eventLogger = eventLogger;
            _debugManager = debugManager;
            _echo = echo;
        }

        public void Render(string version, int tickCount, HardwareSnapshot hardware, DiagnosticSnapshot diagnostic, PhysicsEngineModel physicsEngineModel, CapabilitySnapshot capability)
        {
            string maintenance = BuildMaintenanceText(version, tickCount, hardware, diagnostic);

            WriteSurface(_configuration.FlightDisplayTag, BuildFlightText(version, tickCount, hardware, diagnostic));
            WriteSurface(_configuration.MaintenanceDisplayTag, maintenance);
            WriteSurface(_configuration.EngineeringDisplayTag, _debugManager.Enabled
                ? BuildDebugText(version, tickCount, hardware, diagnostic, physicsEngineModel, capability)
                : BuildEngineeringText(version, tickCount, hardware, diagnostic, physicsEngineModel, capability));
            EchoMaintenanceSummary(version, tickCount, hardware, diagnostic);
        }

        string BuildFlightText(string version, int tickCount, HardwareSnapshot hardware, DiagnosticSnapshot diagnostic)
        {
            _builder.Clear();
            _builder.AppendLine("AAC FLIGHT");
            _builder.AppendLine("v" + version + "  MONITOR ONLY");
            _builder.AppendLine("----------------------");
            _builder.AppendLine("POST : " + diagnostic.Level);
            _builder.AppendLine("CTRL : " + ShortName(hardware.PrimaryControllerName, 20));
            _builder.AppendLine("GRAV : " + FormatCount(hardware.GravityGeneratorCount));
            _builder.AppendLine("MASS : " + FormatCount(hardware.ArtificialMassCount));
            _builder.AppendLine("TICK : " + FormatTick(tickCount));
            _builder.AppendLine();
            _builder.AppendLine(ShortLine(diagnostic.Message, 40));
            return _builder.ToString();
        }

        string BuildMaintenanceText(
            string version,
            int tickCount,
            HardwareSnapshot hardware,
            DiagnosticSnapshot diagnostic)
        {
            _builder.Clear();
            _builder.AppendLine("AAC MAINTENANCE");
            _builder.AppendLine("v" + version + "  MONITOR ONLY");
            _builder.AppendLine("Tick " + FormatTick(tickCount));
            _builder.AppendLine("----------------------------");
            _builder.AppendLine("POST      : " + diagnostic.Level);
            _builder.AppendLine("Controller: " + YesNo(diagnostic.HasController));
            _builder.AppendLine("Drive Pair: " + YesNo(diagnostic.HasGravityDrivePair));
            _builder.AppendLine("AAC LCD   : " + YesNo(diagnostic.HasDisplay));
            _builder.AppendLine("Finding   :");
            AppendWrapped(_builder, diagnostic.Message, "  ", 26);
            _builder.AppendLine();
            _builder.AppendLine("Hardware Counts");
            _builder.AppendLine("  Controllers : " + FormatCount(hardware.ControllerCount));
            _builder.AppendLine("  Gravity Gen : " + FormatCount(hardware.GravityGeneratorCount));
            _builder.AppendLine("  Art. Mass   : " + FormatCount(hardware.ArtificialMassCount));
            _builder.AppendLine("AAC-Owned Propulsion");
            _builder.AppendLine("  Tagged Gen  : " + FormatCount(hardware.TaggedGravityGeneratorCount));
            _builder.AppendLine("  Tagged Mass : " + FormatCount(hardware.TaggedArtificialMassCount));
            _builder.AppendLine("  Alarms      : " + FormatCount(hardware.AlarmCount));
            _builder.AppendLine("  Warn Lights : " + FormatCount(hardware.WarningLightCount));
            _builder.AppendLine("  Displays F/M/E: "
                + hardware.FlightDisplayCount
                + "/"
                + hardware.MaintenanceDisplayCount
                + "/"
                + hardware.EngineeringDisplayCount);
            _builder.AppendLine();
            _eventLogger.AppendTo(_builder);
            return _builder.ToString();
        }

        string BuildEngineeringText(
            string version,
            int tickCount,
            HardwareSnapshot hardware,
            DiagnosticSnapshot diagnostic,
            PhysicsEngineModel physicsEngineModel,
            CapabilitySnapshot capability)
        {
            _builder.Clear();
            _builder.AppendLine("AAC ENGINEERING");
            _builder.AppendLine("v" + version + "  MONITOR ONLY");
            _builder.AppendLine("Tick " + FormatTick(tickCount));
            _builder.AppendLine("----------------------------");
            _builder.AppendLine("Runtime");
            _builder.AppendLine("  Discovery : active");
            _builder.AppendLine("  Diagnostics: active");
            _builder.AppendLine("  Solver    : disabled");
            _builder.AppendLine("  Outputs   : monitor only");
            _builder.AppendLine();
            _builder.AppendLine("Physics Engine Model");
            _builder.AppendLine("  Health    : " + (physicsEngineModel.Ready ? "READY" : "LIMITED"));
            _builder.AppendLine("  Gen D/T/C/N: " + FormatCount(hardware.GravityGeneratorCount) + "/" + FormatCount(physicsEngineModel.TaggedGravityGeneratorCount) + "/" + FormatCount(physicsEngineModel.ContributingGeneratorCount) + "/" + FormatCount(physicsEngineModel.NonContributingGeneratorCount));
            _builder.AppendLine("  AM  D/T/C/N: " + FormatCount(hardware.ArtificialMassCount) + "/" + FormatCount(physicsEngineModel.TaggedArtificialMassCount) + "/" + FormatCount(physicsEngineModel.ContributingMassCount) + "/" + FormatCount(physicsEngineModel.NonContributingMassCount));
            _builder.AppendLine("  Coords    : " + (physicsEngineModel.CoordinateFrameValid ? "VALID" : "INVALID"));
            _builder.AppendLine("  Reference : " + ShortName(physicsEngineModel.ReferenceControllerName, 18));
            _builder.AppendLine();
            _builder.AppendLine("Capability Analysis");
            _builder.AppendLine("  Status    : " + capability.Status);
            _builder.AppendLine("  Redundancy:");
            AppendAxisSummary(_builder, capability);
            _builder.AppendLine("  Control Output: LOCKED");
            AppendWrapped(_builder, capability.Message, "  ", 26);
            _builder.AppendLine();
            _builder.AppendLine("Discovery Snapshot");
            _builder.AppendLine("  Text LCDs : " + FormatCount(hardware.TextPanelCount));
            _builder.AppendLine("  POST      : " + diagnostic.Level);
            _builder.AppendLine("  Detected Gen/Mass: "
                + FormatCount(hardware.GravityGeneratorCount)
                + "/"
                + FormatCount(hardware.ArtificialMassCount));
            return _builder.ToString();
        }


        string BuildDebugText(
            string version,
            int tickCount,
            HardwareSnapshot hardware,
            DiagnosticSnapshot diagnostic,
            PhysicsEngineModel physicsEngineModel,
            CapabilitySnapshot capability)
        {
            _builder.Clear();
            _builder.AppendLine("AAC DEBUG");
            _builder.AppendLine("v" + version + "  MONITOR ONLY");
            _builder.AppendLine("Tick " + FormatTick(tickCount));
            _builder.AppendLine(_debugManager.StatusLine());
            _builder.AppendLine("----------------------------");

            if (_debugManager.InspectingGenerators)
                AppendBlockInspector(_builder, "Generator Inspector", physicsEngineModel.GravityGenerators, true);
            else if (_debugManager.InspectingMass)
                AppendBlockInspector(_builder, "Artificial Mass Inspector", physicsEngineModel.ArtificialMass, false);
            else
            {
            int page = _debugManager.PageIndex;
            if (page == 0)
                AppendDebugOverview(_builder, hardware, diagnostic, physicsEngineModel, capability);
            else if (page == 1)
                AppendDebugDiscovery(_builder, hardware, diagnostic);
            else if (page == 2)
                AppendDebugPemSummary(_builder, physicsEngineModel);
            else if (page == 3)
                AppendDebugCapability(_builder, capability);
            else
                AppendDebugPerformance(_builder, tickCount);
            }

            return _builder.ToString();
        }

        void AppendDebugOverview(StringBuilder builder, HardwareSnapshot hardware, DiagnosticSnapshot diagnostic, PhysicsEngineModel model, CapabilitySnapshot capability)
        {
            builder.AppendLine("Overview");
            builder.AppendLine("  POST      : " + diagnostic.Level);
            builder.AppendLine("  PEM Ready : " + YesNo(model.Ready));
            builder.AppendLine("  Capability: " + capability.Status);
            builder.AppendLine("  Output    : LOCKED");
            builder.AppendLine("  Control   : MONITOR ONLY");
            builder.AppendLine("  Reference : " + ShortName(model.ReferenceControllerName, 18));
        }

        void AppendDebugDiscovery(StringBuilder builder, HardwareSnapshot hardware, DiagnosticSnapshot diagnostic)
        {
            builder.AppendLine("Discovery");
            builder.AppendLine("  Controllers : " + FormatCount(hardware.ControllerCount));
            builder.AppendLine("  Gravity Gen : " + FormatCount(hardware.GravityGeneratorCount));
            builder.AppendLine("  Art. Mass   : " + FormatCount(hardware.ArtificialMassCount));
            builder.AppendLine("  Text LCDs   : " + FormatCount(hardware.TextPanelCount));
            builder.AppendLine("  Alarms      : " + FormatCount(hardware.AlarmCount));
            builder.AppendLine("  Warn Lights : " + FormatCount(hardware.WarningLightCount));
            builder.AppendLine("  Finding:");
            AppendWrapped(builder, diagnostic.Message, "    ", 24);
        }

        void AppendDebugPemSummary(StringBuilder builder, PhysicsEngineModel model)
        {
            builder.AppendLine("PEM Summary");
            builder.AppendLine("  Authority : AAC-owned hardware");
            builder.AppendLine("  Ready     : " + YesNo(model.Ready));
            builder.AppendLine("  Coords    : " + (model.CoordinateFrameValid ? "VALID" : "INVALID"));
            builder.AppendLine("  Reference : " + ShortName(model.ReferenceControllerName, 18));
            builder.AppendLine("  Gen T/C/N : " + FormatCount(model.TaggedGravityGeneratorCount) + "/" + FormatCount(model.ContributingGeneratorCount) + "/" + FormatCount(model.NonContributingGeneratorCount));
            builder.AppendLine("  AM  T/C/N : " + FormatCount(model.TaggedArtificialMassCount) + "/" + FormatCount(model.ContributingMassCount) + "/" + FormatCount(model.NonContributingMassCount));
        }

        void AppendBlockInspector(StringBuilder builder, string title, List<HardwareBlockMetadata> blocks, bool generator)
        {
            builder.AppendLine(title);
            builder.AppendLine("  Count: " + FormatCount(blocks.Count));
            if (blocks.Count == 0)
            {
                builder.AppendLine("  none");
                return;
            }
            int index = _debugManager.ActiveOrdinal(blocks.Count);
            HardwareBlockMetadata block = blocks[index];
            builder.AppendLine("  Page: " + (index + 1) + "/" + blocks.Count);
            builder.AppendLine("  Stable Debug ID : " + block.DebugId);
            builder.AppendLine("  Entity ID       : " + block.EntityId);
            builder.AppendLine("  Custom Name     : " + ShortName(block.CustomName, 28));
            builder.AppendLine("  Mount Position  : " + block.MountPosition);
            if (generator)
            {
                builder.AppendLine("  Block Orient.   : " + block.BlockOrientation);
                builder.AppendLine("  Gravity Axis    : " + block.GravityProjectionAxis);
            }
            builder.AppendLine("  Distance        : " + block.DistanceFromController.ToString("0.0"));
            builder.AppendLine("  Enabled         : " + YesNo(block.Enabled));
            builder.AppendLine("  Working         : " + YesNo(block.Working));
            builder.AppendLine("  Validated       : " + YesNo(block.Validated));
            builder.AppendLine("  Contributing    : " + YesNo(block.Contributing));
        }

        void AppendDebugCapability(StringBuilder builder, CapabilitySnapshot capability)
        {
            builder.AppendLine("Capability Analysis");
            builder.AppendLine("  Status    : " + capability.Status);
            builder.AppendLine("  PEM Ready : " + YesNo(capability.PemReady));
            builder.AppendLine("  Output    : LOCKED");
            builder.AppendLine("  Axes:");
            AppendAxisSummary(builder, capability);
            builder.AppendLine("  Message:");
            AppendWrapped(builder, capability.Message, "    ", 24);
        }


        void AppendAxisSummary(StringBuilder builder, CapabilitySnapshot capability)
        {
            for (int i = 0; i < capability.Axes.Count; i++)
            {
                AxisCapability axis = capability.Axes[i];
                builder.AppendLine("    " + axis.Axis + " READY " + YesNo(axis.Ready) + " Gen " + FormatCount(axis.GeneratorCount) + " Tol " + FormatCount(axis.ToleranceCount));
                if (!axis.Ready)
                    builder.AppendLine("      " + axis.Reason);
            }
        }

        void AppendDebugPerformance(StringBuilder builder, int tickCount)
        {
            builder.AppendLine("Performance Placeholder");
            builder.AppendLine("  Profiler  : not implemented");
            builder.AppendLine("  Tick      : " + FormatTick(tickCount));
            builder.AppendLine("  Cadence   : Update100");
            builder.AppendLine("  Impact    : read-only display");
        }

        void EchoMaintenanceSummary(string version, int tickCount, HardwareSnapshot hardware, DiagnosticSnapshot diagnostic)
        {
            if (_echo == null)
                return;

            _builder.Clear();
            _builder.AppendLine("AAC v" + version + " MONITOR ONLY");
            _builder.AppendLine("Tick " + FormatTick(tickCount) + " POST " + diagnostic.Level);
            _builder.AppendLine("Ctrl " + hardware.ControllerCount
                + " Grav " + hardware.GravityGeneratorCount
                + " Mass " + hardware.ArtificialMassCount);
            _builder.AppendLine(_debugManager.StatusLine());
            _builder.AppendLine(ShortLine(diagnostic.Message, 36));
            _echo(_builder.ToString());
        }

        static string FormatCount(int value)
        {
            return value.ToString("000");
        }

        static string FormatTick(int value)
        {
            return value.ToString("00000");
        }

        static string YesNo(bool value)
        {
            return value ? "YES" : "NO";
        }

        static string ShortName(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return "none";
            return ShortLine(value, maxLength);
        }

        static string ShortLine(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value;
            if (maxLength <= 3)
                return value.Substring(0, maxLength);
            return value.Substring(0, maxLength - 3) + "...";
        }

        static void AppendWrapped(
            StringBuilder builder,
            string text,
            string indent,
            int maxWidth)
        {
            if (string.IsNullOrEmpty(text))
            {
                builder.AppendLine(indent + "none");
                return;
            }

            int index = 0;
            while (index < text.Length)
            {
                int take = Math.Min(maxWidth, text.Length - index);
                builder.AppendLine(indent + text.Substring(index, take));
                index += take;
            }
        }

        void WriteSurface(string tag, string text)
        {
            _panels.Clear();
            _gridTerminalSystem.GetBlocksOfType(
                _panels,
                panel => panel.IsSameConstructAs(_me)
                    && panel.CustomName.IndexOf(tag, StringComparison.OrdinalIgnoreCase) >= 0);
            for (int i = 0; i < _panels.Count; i++)
            {
                _panels[i].ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                _panels[i].Font = "Debug";
                _panels[i].FontSize = 0.65f;
                _panels[i].WriteText(text, false);
            }
        }
    }

    sealed class EventLogger
    {
        readonly int _capacity;
        readonly Queue<string> _events = new Queue<string>();

        public EventLogger(int capacity)
        {
            _capacity = Math.Max(1, capacity);
        }

        public void Record(int tick, string message)
        {
            if (_events.Count >= _capacity)
                _events.Dequeue();

            _events.Enqueue("[" + tick.ToString("00000") + "] " + message);
        }

        public void AppendTo(StringBuilder builder)
        {
            builder.AppendLine("Events:");
            if (_events.Count == 0)
            {
                builder.AppendLine("- none");
                return;
            }

            foreach (string item in _events)
                builder.AppendLine("- " + item);
        }
    }
