/*
===============================================================================
 Adaptive Antigravity Controller (AAC)
 Version: 0.1.0-alpha.1
 Project Lead: Nomaddison
 Development Assistance: OpenAI ChatGPT

 MILESTONE 1 FOUNDATION BUILD
 This script establishes the runtime shell for discovery, diagnostics, and
 operator displays while keeping propulsion outputs in monitor-only mode.
===============================================================================
*/

    const string Version = "0.1.0-alpha.1";
    const string SystemTag = "[AAC]";

    readonly Configuration _configuration;
    readonly EventLogger _eventLogger;
    readonly HardwareDiscovery _hardwareDiscovery;
    readonly Diagnostics _diagnostics;
    readonly DisplayManager _displayManager;
    readonly AacCore _core;

    public Program()
    {
        Runtime.UpdateFrequency = UpdateFrequency.Update100;

        _configuration = new Configuration(SystemTag);
        _eventLogger = new EventLogger(24);
        _hardwareDiscovery = new HardwareDiscovery(GridTerminalSystem, Me, _configuration);
        _diagnostics = new Diagnostics();
        _displayManager = new DisplayManager(GridTerminalSystem, Me, _configuration, _eventLogger);
        _core = new AacCore(Version, _hardwareDiscovery, _diagnostics, _displayManager, _eventLogger);

        _eventLogger.Record(0, "AAC boot: monitor-only foundation online.");
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
        readonly DisplayManager _displayManager;
        readonly EventLogger _eventLogger;
        int _tickCount;

        public AacCore(
            string version,
            HardwareDiscovery hardwareDiscovery,
            Diagnostics diagnostics,
            DisplayManager displayManager,
            EventLogger eventLogger)
        {
            _version = version;
            _hardwareDiscovery = hardwareDiscovery;
            _diagnostics = diagnostics;
            _displayManager = displayManager;
            _eventLogger = eventLogger;
        }

        public void Tick(string command)
        {
            _tickCount++;

            HardwareSnapshot hardware = _hardwareDiscovery.Scan();
            DiagnosticSnapshot diagnostic = _diagnostics.Evaluate(hardware);

            if (IsManualRescan(command))
                _eventLogger.Record(_tickCount, "Manual rescan requested.");

            _displayManager.Render(_version, _tickCount, hardware, diagnostic);
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

            return new HardwareSnapshot(
                _controllers.Count,
                _gravityGenerators.Count,
                _artificialMass.Count,
                _textPanels.Count,
                _alarms.Count,
                _warningLights.Count,
                FirstUsableControllerName(),
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

        string FirstUsableControllerName()
        {
            for (int i = 0; i < _controllers.Count; i++)
            {
                if (_controllers[i].IsMainCockpit || _controllers[i].CanControlShip)
                    return _controllers[i].CustomName;
            }

            return _controllers.Count == 0 ? "none" : _controllers[0].CustomName;
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

    sealed class HardwareSnapshot
    {
        public readonly int ControllerCount;
        public readonly int GravityGeneratorCount;
        public readonly int ArtificialMassCount;
        public readonly int TextPanelCount;
        public readonly int AlarmCount;
        public readonly int WarningLightCount;
        public readonly string PrimaryControllerName;
        public readonly int FlightDisplayCount;
        public readonly int MaintenanceDisplayCount;
        public readonly int EngineeringDisplayCount;

        public HardwareSnapshot(
            int controllerCount,
            int gravityGeneratorCount,
            int artificialMassCount,
            int textPanelCount,
            int alarmCount,
            int warningLightCount,
            string primaryControllerName,
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
            string message = findings.Count == 0 ? "all required hardware discovered" : JoinFindings(findings);

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

    sealed class DisplayManager
    {
        readonly IMyGridTerminalSystem _gridTerminalSystem;
        readonly IMyProgrammableBlock _me;
        readonly Configuration _configuration;
        readonly EventLogger _eventLogger;
        readonly List<IMyTextPanel> _panels = new List<IMyTextPanel>();
        readonly StringBuilder _builder = new StringBuilder();

        public DisplayManager(
            IMyGridTerminalSystem gridTerminalSystem,
            IMyProgrammableBlock me,
            Configuration configuration,
            EventLogger eventLogger)
        {
            _gridTerminalSystem = gridTerminalSystem;
            _me = me;
            _configuration = configuration;
            _eventLogger = eventLogger;
        }

        public void Render(string version, int tickCount, HardwareSnapshot hardware, DiagnosticSnapshot diagnostic)
        {
            WriteSurface(_configuration.FlightDisplayTag, BuildFlightText(version, tickCount, hardware, diagnostic));
            WriteSurface(_configuration.MaintenanceDisplayTag, BuildMaintenanceText(version, hardware, diagnostic));
            WriteSurface(_configuration.EngineeringDisplayTag, BuildEngineeringText(version, hardware));
        }

        string BuildFlightText(string version, int tickCount, HardwareSnapshot hardware, DiagnosticSnapshot diagnostic)
        {
            _builder.Clear();
            _builder.AppendLine("AAC Flight");
            _builder.AppendLine("Version: " + version);
            _builder.AppendLine("Mode: Monitor Only");
            _builder.AppendLine("Status: " + diagnostic.Level);
            _builder.AppendLine("POST: " + diagnostic.Message);
            _builder.AppendLine("Controller: " + hardware.PrimaryControllerName);
            _builder.AppendLine("Gravity generators: " + hardware.GravityGeneratorCount);
            _builder.AppendLine("Artificial mass: " + hardware.ArtificialMassCount);
            _builder.AppendLine("Tick: " + tickCount);
            return _builder.ToString();
        }

        string BuildMaintenanceText(string version, HardwareSnapshot hardware, DiagnosticSnapshot diagnostic)
        {
            _builder.Clear();
            _builder.AppendLine("AAC Maintenance");
            _builder.AppendLine("Version: " + version);
            _builder.AppendLine("POST: " + diagnostic.Level);
            _builder.AppendLine("Findings: " + diagnostic.Message);
            _builder.AppendLine("Controllers: " + hardware.ControllerCount);
            _builder.AppendLine("Gravity generators: " + hardware.GravityGeneratorCount);
            _builder.AppendLine("Artificial mass blocks: " + hardware.ArtificialMassCount);
            _builder.AppendLine("AAC alarms: " + hardware.AlarmCount);
            _builder.AppendLine("AAC warning lights: " + hardware.WarningLightCount);
            _builder.AppendLine(
                "Displays F/M/E: "
                + hardware.FlightDisplayCount
                + "/"
                + hardware.MaintenanceDisplayCount
                + "/"
                + hardware.EngineeringDisplayCount);
            _builder.AppendLine();
            _eventLogger.AppendTo(_builder);
            return _builder.ToString();
        }

        string BuildEngineeringText(string version, HardwareSnapshot hardware)
        {
            _builder.Clear();
            _builder.AppendLine("AAC Engineering");
            _builder.AppendLine("Version: " + version);
            _builder.AppendLine("Architecture: adaptive discovery");
            _builder.AppendLine("PEM: pending calibration");
            _builder.AppendLine("Capability analysis: pending");
            _builder.AppendLine("Gravity solver: disabled");
            _builder.AppendLine("Discovered text panels: " + hardware.TextPanelCount);
            return _builder.ToString();
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

            _events.Enqueue("tick " + tick + ": " + message);
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
