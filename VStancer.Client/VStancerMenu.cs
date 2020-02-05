using System;
using System.Threading.Tasks;
using MenuAPI;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;

namespace Vstancer.Client {
    internal class VStancerMenu : BaseScript {
        #region Private Fields

        /// <summary>
        /// The script which owns this menu
        /// </summary>
        private VStancerEditor vstancerEditor;

        /// <summary>
        /// The controller of the menu
        /// </summary>
        private MenuController menuController;

        /// <summary>
        /// The main menu
        /// </summary>
        private Menu editorMenu;

        #endregion

        #region Public Events

        /// <summary>
        /// Triggered when a property has its value changed in the UI
        /// </summary>
        /// <param name="id">The id of the property</param>
        /// <param name="value">The new value of the property</param>
        public delegate void MenuPresetValueChangedEvent(string id, string value);

        /// <summary>
        /// Triggered when a property has its value changed in the UI
        /// </summary>
        public event MenuPresetValueChangedEvent MenuPresetValueChanged;

        /// <summary>
        /// Triggered when the reset button is pressed in the UI
        /// </summary>
        public event EventHandler MenuResetPresetButtonPressed;

        #endregion

        #region Editor Properties

        private string ResetID => VStancerEditor.ResetID;
        private string SaveID = "vstancer_save_preset";
        private string LoadID = "vstancer_load_preset";
        private string FrontOffsetID => VStancerEditor.FrontOffsetID;
        private string FrontRotationID => VStancerEditor.FrontRotationID;
        private string RearOffsetID => VStancerEditor.RearOffsetID;
        private string RearRotationID => VStancerEditor.RearRotationID;
        private string SteeringLockID => VStancerEditor.SteeringLockID;
        private string SuspensionHeightID => VStancerEditor.SuspensionHeightID;
        private string WheelSizeID => VStancerEditor.WheelSizeID;
        private string WheelWidthID => VStancerEditor.WheelWidthID;
        private string ScriptName => VStancerEditor.ScriptName;
        private float frontMaxOffset => vstancerEditor.frontMaxOffset;
        private float frontMaxCamber => vstancerEditor.frontMaxCamber;
        private float rearMaxOffset => vstancerEditor.rearMaxOffset;
        private float rearMaxCamber => vstancerEditor.rearMaxCamber;
        private float steeringLockMinVal => vstancerEditor.steeringLockMinVal;
        private float steeringLockMaxVal => vstancerEditor.steeringLockMaxVal;
        private float suspensionHeightMinVal => vstancerEditor.suspensionHeightMinVal;
        private float suspensionHeightMaxVal => vstancerEditor.suspensionHeightMaxVal;
        private float wheelSizeMinVal => vstancerEditor.wheelSizeMinVal;
        private float wheelSizeMaxVal => vstancerEditor.wheelSizeMaxVal;
        private float wheelWidthMinVal => vstancerEditor.wheelWidthMinVal;
        private float wheelWidthMaxVal => vstancerEditor.wheelWidthMaxVal;
        private bool CurrentPresetIsValid => vstancerEditor.CurrentPresetIsValid;
        private VStancerPreset currentPreset => vstancerEditor.currentPreset;
        private int toggleMenu => vstancerEditor.toggleMenu;
        private float FloatStep => vstancerEditor.FloatStep;
        private bool enableSH => vstancerEditor.enableSH;
        private bool enableSL => vstancerEditor.enableSL;

        #endregion

        #region Private Methods

        /// <summary>
        /// Create a method to determine the logic for when the left/right arrow are pressed
        /// </summary>
        /// <param name="name">The name of the item</param>
        /// <param name="value">The current value</param>
        /// <param name="minimum">The min allowed value</param>
        /// <param name="maximum">The max allowed value</param>
        /// <returns>The <see cref="MenuDynamicListItem.ChangeItemCallback"/></returns>
        private MenuDynamicListItem.ChangeItemCallback FloatChangeCallback(string name, float value, float minimum, float maximum, float step) {
            string callback(MenuDynamicListItem sender, bool left) {
                var min = minimum;
                var max = maximum;

                var newvalue = value;

                if (left)
                    newvalue -= step;
                else if (!left)
                    newvalue += step;
                else return value.ToString("F3");

                // Hotfix to trim the value to 3 digits
                newvalue = float.Parse((newvalue).ToString("F3"));

                if (newvalue < min)
                    Screen.ShowNotification($"~o~Warning~w~: Min ~b~{name}~w~ value allowed is {min} for this vehicle");
                else if (newvalue > max)
                    Screen.ShowNotification($"~o~Warning~w~: Max ~b~{name}~w~ value allowed is {max} for this vehicle");
                else {
                    value = newvalue;
                }
                return value.ToString("F3");
            };
            return callback;
        }

        /// <summary>
        /// Creates a controller for the a float property
        /// </summary>
        /// <param name="menu">The menu to add the controller to</param>
        /// <param name="name">The displayed name of the controller</param>
        /// <param name="defaultValue">The default value of the controller</param>
        /// <param name="value">The current value of the controller</param>
        /// <param name="maxEditing">The max delta allowed relative to the default value</param>
        /// <param name="id">The ID of the property linked to the controller</param>
        /// <returns></returns>
        private MenuDynamicListItem AddDynamicFloatList(Menu menu, string name, float defaultValue, float value, float maxEditing, string id) {
            float min = defaultValue - maxEditing;
            float max = defaultValue + maxEditing;

            var callback = FloatChangeCallback(name, value, min, max, FloatStep);

            var newitem = new MenuDynamicListItem(name, value.ToString("F3"), callback) { ItemData = id };
            menu.AddMenuItem(newitem);
            return newitem;
        }

        /// <summary>
        /// Setup the menu
        /// </summary>
        private void InitializeMenu() {
            if (editorMenu == null) {
                editorMenu = new Menu(ScriptName, "Editor");

                // When the value of a MenuDynamicListItem is changed
                editorMenu.OnDynamicListItemCurrentItemChange += (menu, dynamicListItem, oldValue, newValue) => {
                    string id = dynamicListItem.ItemData as string;
                    MenuPresetValueChanged?.Invoke(id, newValue);
                };

                editorMenu.OnMenuOpen += (menu) => {
                    UpdateWheelValues();
                    UpdateEditorMenu((currentPreset.WheelSize==0.0f), (currentPreset.WheelWidth == 0.0f));
                };

                // When a MenuItem is selected
                editorMenu.OnItemSelect += (menu, menuItem, itemIndex) => {
                    // If the selected item is the reset button
                    if (menuItem.ItemData as string == ResetID) {
                        MenuResetPresetButtonPressed.Invoke(this, EventArgs.Empty);
                    } else if (menuItem.ItemData as string == SaveID) {
                        SavePreset();
                    } else if (menuItem.ItemData as string == LoadID) {
                        LoadPreset();
                    }
                };
            }

            UpdateEditorMenu();

            if (menuController == null) {
                menuController = new MenuController();
                MenuController.AddMenu(editorMenu);
                MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Right;
                MenuController.MenuToggleKey = (Control)toggleMenu;
                MenuController.EnableMenuToggleKeyOnController = false;
                MenuController.MainMenu = editorMenu;
            }
        }

        /// <summary>
        /// Update the items of the main menu
        /// </summary>
        private void UpdateEditorMenu(bool hideWheelSize = false, bool hideWheelWidth = false) {
            if (editorMenu == null)
                return;

            editorMenu.ClearMenuItems();

            if (!CurrentPresetIsValid)
                return;

            AddDynamicFloatList(editorMenu, "Front Track Width", -currentPreset.DefaultOffsetX[0], -currentPreset.OffsetX[0], frontMaxOffset, FrontOffsetID);
            AddDynamicFloatList(editorMenu, "Rear Track Width", -currentPreset.DefaultOffsetX[currentPreset.FrontWheelsCount], -currentPreset.OffsetX[currentPreset.FrontWheelsCount], rearMaxOffset, RearOffsetID);
            AddDynamicFloatList(editorMenu, "Front Camber", currentPreset.DefaultRotationY[0], currentPreset.RotationY[0], frontMaxCamber, FrontRotationID);
            AddDynamicFloatList(editorMenu, "Rear Camber", currentPreset.DefaultRotationY[currentPreset.FrontWheelsCount], currentPreset.RotationY[currentPreset.FrontWheelsCount], rearMaxCamber, RearRotationID);
            // Steering lock, custom min max
            if (enableSL) {
                var callbackSL = FloatChangeCallback("Steering Lock", currentPreset.SteeringLock, steeringLockMinVal, steeringLockMaxVal, 1f);
                var newitemSL = new MenuDynamicListItem("Steering Lock", currentPreset.SteeringLock.ToString("F3"), callbackSL) { ItemData = SteeringLockID };
                editorMenu.AddMenuItem(newitemSL);
            }
            // Suspension height, custom min max
            if (enableSH) {
                var callbackSH = FloatChangeCallback("Suspension Height", currentPreset.SuspensionHeight, suspensionHeightMinVal, suspensionHeightMaxVal, 0.005f);
                var newitemSH = new MenuDynamicListItem("Suspension Height", currentPreset.SuspensionHeight.ToString("F3"), callbackSH) { ItemData = SuspensionHeightID };
                editorMenu.AddMenuItem(newitemSH);
            }
            // Wheel size, custom min max
            var callbackWS = FloatChangeCallback("Wheel size", currentPreset.WheelSize, wheelSizeMinVal, wheelSizeMaxVal, 0.025f);
            var newitemWS = new MenuDynamicListItem("Wheel size", currentPreset.WheelSize.ToString("F3"), callbackWS, "Only works on non-default wheels") { ItemData = WheelSizeID };
            editorMenu.AddMenuItem(newitemWS);
            // Wheel width, custom min max
            var callbackWW = FloatChangeCallback("Wheel width", currentPreset.WheelWidth, wheelWidthMinVal, wheelWidthMaxVal, 0.025f);
            var newitemWW = new MenuDynamicListItem("Wheel width", currentPreset.WheelWidth.ToString("F3"), callbackWW, "Only works on non-default wheels") { ItemData = WheelWidthID };
            editorMenu.AddMenuItem(newitemWW);

            if (hideWheelSize) {
                newitemWS.Enabled = false;
            } else {
                newitemWS.Enabled = true;
            }
            if (hideWheelWidth) {
                newitemWW.Enabled = false;
            } else {
                newitemWW.Enabled = true;
            }

            editorMenu.AddMenuItem(new MenuItem("Reset", "Restores the default values") { ItemData = ResetID });
            editorMenu.AddMenuItem(new MenuItem("Save preset", "Saves preset for this vehicle") { ItemData = SaveID });
            editorMenu.AddMenuItem(new MenuItem("Load preset", "Loads preset for this vehicle") { ItemData = LoadID });
            editorMenu.AddMenuItem(new MenuItem("Credits",  "~g~Carmineos~g~~w~ - Author of vStancer~w~\n" +
                                                            "~y~Tom Grobbe~y~~w~ - MenuAPI used for GUI~w~\n" +
                                                            "~y~Shrimp~y~~w~ - Additional functionality (SL, SH, wheel size/width, presets)~w~\n" + " \n"));
        }

        private void UpdateWheelValues(){
            var playerPed = PlayerPedId();
            if (IsPedInAnyVehicle(playerPed, false)){
                int vehicle = GetVehiclePedIsIn(playerPed, false);
                if (IsThisModelACar((uint)GetEntityModel(vehicle)) && GetPedInVehicleSeat(vehicle, -1) == playerPed && IsVehicleDriveable(vehicle, false)) {
                    if(currentPreset.WheelSize == 0.0f)
                        currentPreset.WheelSize = GetVehicleWheelSize(vehicle);
                    if (currentPreset.WheelWidth == 0.0f)
                        currentPreset.WheelWidth = GetVehicleWheelWidth(vehicle);
                    if (currentPreset.DefaultWheelSize == 0.0f && GetVehicleWheelSize(vehicle) != 0.0f) {
                        currentPreset.DefaultWheelSize = GetVehicleWheelSize(vehicle);
                    }
                    if (currentPreset.DefaultWheelWidth == 0.0f && GetVehicleWheelWidth(vehicle) != 0.0f) {
                        currentPreset.DefaultWheelWidth = GetVehicleWheelWidth(vehicle);
                    }
                }
            }
        }

        private void SavePreset()
        {
            var playerPed = PlayerPedId();

            if (IsPedInAnyVehicle(playerPed, false))
            {
                int vehicle = GetVehiclePedIsIn(playerPed, false);
                if (vehicle >= 0)
                {
                    float[] preset = vstancerEditor.GetVstancerPreset(vehicle);
                    if (preset.Length > 6)
                    {
                        string name = GetDisplayNameFromVehicleModel((uint)GetEntityModel(vehicle));
                        if (SavePresetAsKVP(name, preset))
                        {
                            Debug.WriteLine($"[vStancer] Saved preset for " + name + "!\n");
                        }
                        else
                        {
                            Debug.WriteLine($"[vStancer] Failed to save preset for " + name + "!\n");
                        }
                    }
                }
            }
        }

        private async Task LoadPreset()
        {
            var playerPed = PlayerPedId();

            // Make sure car is spawned
            await Delay(1000);

            if (IsPedInAnyVehicle(playerPed, false))
            {
                int vehicle = GetVehiclePedIsIn(playerPed, false);
                if (vehicle >= 0)
                {
                    string name = GetDisplayNameFromVehicleModel((uint)GetEntityModel(vehicle));
                    float[] loadedPreset = LoadPresetFromKVP(name);
                    if (loadedPreset != null)
                        if (loadedPreset.Length > 6)
                        {
                            float wheelSizeTemp = loadedPreset[6];
                            if (wheelSizeTemp < 0.0f) {
                                wheelSizeTemp *= -1.0f;
                            }

                            float wheelWidthTemp = loadedPreset[7];
                            if (wheelWidthTemp < 0.0f) {
                                wheelWidthTemp *= -1.0f;
                            }
                            vstancerEditor.SetVstancerPreset(vehicle, loadedPreset[0], loadedPreset[1], loadedPreset[2], loadedPreset[3], loadedPreset[4], loadedPreset[5], wheelSizeTemp, wheelWidthTemp);
                        } else {
                            vstancerEditor.SetVstancerPreset(vehicle, loadedPreset[0], loadedPreset[1], loadedPreset[2], loadedPreset[3], loadedPreset[4], loadedPreset[5], 0.0f, 0.0f);
                        }
                        Debug.WriteLine($"[vStancer] Loaded preset for " + name + "!");
                }
            }
            await Delay(0);
        }

        private bool SavePresetAsKVP(string name, float[] preset)
        {
            if (!string.IsNullOrEmpty(name))
            {
                // Convert
                string json = JsonConvert.SerializeObject(preset);

                // Log
                Debug.WriteLine($"[vStancer] Saving preset for " + name + "...");

                // Save
                SetResourceKvp("vStancer_PRESET_" + name, json);

                // confirm
                return GetResourceKvpString("vStancer_PRESET_" + name) == json;
            }
            else
            {
                return false;
            }
        }

        private float[] LoadPresetFromKVP(string name)
        {
            Debug.WriteLine($"[vStancer] Loading preset for " + name + "...");
            if (GetResourceKvpString("vStancer_PRESET_" + name) != null)
            {
                return (float[])JsonConvert.DeserializeObject<float[]>(GetResourceKvpString("vStancer_PRESET_" + name));
            }
            else
            {
                Debug.WriteLine($"[vStancer] Failed to load preset for " + name + "...");
                return null;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="script">The script which owns this menu</param>
        internal VStancerMenu(VStancerEditor script)
        {
            vstancerEditor = script;
            vstancerEditor.PresetChanged += new EventHandler((sender,args) => UpdateEditorMenu());
            vstancerEditor.ToggleMenuVisibility += new EventHandler((sender,args) => 
            {
                if (editorMenu == null)
                    return;

                editorMenu.Visible = !editorMenu.Visible;
            });
            InitializeMenu();
            
            Exports.Add("LoadVStancerPreset", new Func<Task>(LoadPreset));

            Tick += OnTick;
        }

        #endregion

        #region Tasks

        /// <summary>
        /// The task that checks if the menu can be open
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            if (!CurrentPresetIsValid)
            {
                if (MenuController.IsAnyMenuOpen())
                    MenuController.CloseAllMenus();
            }

            await Task.FromResult(0);
        }

        #endregion
    }
}
