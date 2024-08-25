﻿using SDG.Unturned;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Thanking.Attributes;
using Thanking.Coroutines;
using Thanking.Options;
using Thanking.Options.AimOptions;
using Thanking.Utilities;
using Thanking.Variables;
using UnityEngine;

namespace Thanking.Components.Basic
{
	[Component]
    public class MiscComponent : MonoBehaviour
    {
        public static Vector3 LastDeath;
        public static GameObject dupeplayer;
        public static MiscComponent Instance;
        public static float LastMovementCheck;
        public static bool FreecamBeforeSpy;
        public static bool NightvisionBeforeSpy;
        private static bool zoomedBeforeSpy;
        private static bool isZoomed;
        public static List<PlayerInputPacket> ClientsidePackets;

        public static FieldInfo Primary =
            typeof(PlayerEquipment).GetField("_primary", BindingFlags.NonPublic | BindingFlags.Instance);

        public static FieldInfo Sequence =
            typeof(PlayerInput).GetField("sequence", BindingFlags.NonPublic | BindingFlags.Instance);

        public static FieldInfo CPField =
            typeof(PlayerInput).GetField("clientsidePackets", BindingFlags.NonPublic | BindingFlags.Instance);

        private static FieldInfo FOVField =
            typeof(PlayerLook).GetField("fov", BindingFlags.NonPublic | BindingFlags.Instance);

        private static FieldInfo IsZoomedField =
            typeof(PlayerLook).GetField("isZoomed", BindingFlags.NonPublic | BindingFlags.Instance);

        private static float PlayerLookFov
        {
            get => (float)FOVField.GetValue(Player.player.look);
            set => FOVField.SetValue(Player.player.look, value);
        }

        private static bool PlayerLookZoom
        {
            get => (bool)IsZoomedField.GetValue(Player.player.look);
            set => IsZoomedField.SetValue(Player.player.look, value);
        }

        private int currentKills = -1;

        [Initializer]
        public static void Initialize()
        {
            HotkeyComponent.ActionDict.Add("_VFToggle", () =>
                MiscOptions.VehicleFly = !MiscOptions.VehicleFly);

            HotkeyComponent.ActionDict.Add("_ToggleAimbot", () =>
                AimbotOptions.Enabled = !AimbotOptions.Enabled);

            HotkeyComponent.ActionDict.Add("_AimbotOnKey", () =>
                AimbotOptions.OnKey = !AimbotOptions.OnKey);

            HotkeyComponent.ActionDict.Add("_ToggleFreecam", () =>
                MiscOptions.Freecam = !MiscOptions.Freecam);

            HotkeyComponent.ActionDict.Add("_PanicButton", () =>
            {
                MiscOptions.PanicMode = !MiscOptions.PanicMode;
                if (MiscOptions.PanicMode)
                    PlayerCoroutines.DisableAllVisuals();
                else
                    PlayerCoroutines.EnableAllVisuals();
            });

            HotkeyComponent.ActionDict.Add("_SelectPlayer", () =>
            {
                Vector3 aimPos = Player.player.look.aim.position;
                Vector3 aimForward = Player.player.look.aim.forward;

                if (RaycastOptions.EnablePlayerSelection)
                {
                    foreach (GameObject o in RaycastUtilities.Objects)
                    {
                        Player player = o.GetComponent<Player>();
                        if (player != null)
                        {
                            if (VectorUtilities.GetAngleDelta(aimPos, aimForward, o.transform.position) < RaycastOptions.SelectedFOV)
                            {
                                RaycastUtilities.TargetedPlayer = player;
                                break;
                            }
                        }
                    }
                }
            });

            HotkeyComponent.ActionDict.Add("_InstantDisconnect", () => Provider.disconnect());
        }

        [OnSpy]
        public static void Disable()
        {
            if (MiscOptions.WasNightVision)
            {
                NightvisionBeforeSpy = true;
                MiscOptions.NightVision = false;
            }

            if (MiscOptions.Freecam)
            {
                FreecamBeforeSpy = true;
                MiscOptions.Freecam = false;
            }

            zoomedBeforeSpy = isZoomed;

            if (isZoomed)
            {
                PlayerLookFov = 0F;
                PlayerLookZoom = false;
                InstantZoom(0F);
            }

            isZoomed = false;
        }

        [OffSpy]
        public static void Enable()
        {
            if (NightvisionBeforeSpy)
            {
                NightvisionBeforeSpy = false;
                MiscOptions.NightVision = true;
            }

            if (FreecamBeforeSpy)
            {
                FreecamBeforeSpy = false;
                MiscOptions.Freecam = true;
            }

            isZoomed = zoomedBeforeSpy;

            if (isZoomed)
            {
                PlayerLookFov = MiscOptions.ZoomFOV;
                PlayerLookZoom = true;
                InstantZoom(MiscOptions.ZoomFOV);
            }
        }

        void Start()
        {
            Instance = this;

            Provider.onClientConnected += () =>
            {
                if (MiscOptions.AlwaysCheckMovementVerification)
                    CheckMovementVerification();
                else
                    MiscOptions.NoMovementVerification = false;
            };

            SkinsUtilities.RefreshEconInfo();
        }

        public void Update()
        {
            if (Camera.main != null && MainCamera.instance == null)
                MainCamera.instance = Camera.main;

            if (!Player.player)
                return;

            if (!DrawUtilities.ShouldRun())
                return;

            if (MiscOptions.NightVision)
            {
                LevelLighting.vision = ELightingVision.MILITARY;
                LevelLighting.updateLighting();
                PlayerLifeUI.updateGrayscale();
                MiscOptions.WasNightVision = true;
            }
            else if (MiscOptions.WasNightVision)
            {
                LevelLighting.vision = ELightingVision.NONE;
                LevelLighting.updateLighting();
                PlayerLifeUI.updateGrayscale();
                MiscOptions.WasNightVision = false;
            }

            if (Player.player.life.isDead)
                LastDeath = Player.player.transform.position;

            if (MiscOptions.ZoomOnHotkey && !isZoomed && HotkeyUtilities.IsHotkeyHeld("_Zoom") && !PlayerLookZoom)
            {
                isZoomed = true;
                PlayerLookFov = MiscOptions.ZoomFOV;
                PlayerLookZoom = true;

                if (MiscOptions.InstantZoom)
                    InstantZoom(MiscOptions.ZoomFOV);
            }
            else if (isZoomed && !HotkeyUtilities.IsHotkeyHeld("_Zoom"))
            {
                isZoomed = false;
                PlayerLookFov = 0F;
                PlayerLookZoom = false;

                if (MiscOptions.InstantZoom)
                    InstantZoom(0F);
            }

            if (MiscOptions.AutoJoin)
            {
                string[] ipinfo = MiscOptions.AutoJoinIP.Split(':');  
                Provider.provider.matchmakingService.connect(new SteamConnectionInfo(ushort.Parse(ipinfo[0]), ushort.Parse(ipinfo[1]), ""));
            }
        }

        private static void InstantZoom(float fov)
        {
            if (fov <= 1F)
                MainCamera.instance.fieldOfView = OptionsSettings._cachedVerticalFOV + (Player.player.stance.stance != EPlayerStance.SPRINT ? 0F : 10F);
            else
                MainCamera.instance.fieldOfView = fov;

            Player.player.look.characterCamera.fieldOfView = MainCamera.instance.fieldOfView;
        }

        public void FixedUpdate()
        {
            if (!Player.player)
                return;

            VehicleFlight();
            PlayerFlight();
        }

        public static void PlayerFlight()
        {
            Player plr = Player.player;

            if (!MiscOptions.PlayerFlight)
            {
                ItemCloudAsset asset = plr.equipment.asset as ItemCloudAsset;
                plr.movement.itemGravityMultiplier = asset?.gravity ?? 1;
                return;
            }

            plr.movement.itemGravityMultiplier = 0;

            float multiplier = MiscOptions.FlightSpeedMultiplier;

            if (HotkeyUtilities.IsHotkeyHeld("_FlyUp"))
                plr.transform.position += plr.transform.up / 5 * multiplier;

            if (HotkeyUtilities.IsHotkeyHeld("_FlyDown"))
                plr.transform.position -= plr.transform.up / 5 * multiplier;

            if (HotkeyUtilities.IsHotkeyHeld("_FlyLeft"))
                plr.transform.position -= plr.transform.right / 5 * multiplier;

            if (HotkeyUtilities.IsHotkeyHeld("_FlyRight"))
                plr.transform.position += plr.transform.right / 5 * multiplier;

            if (HotkeyUtilities.IsHotkeyHeld("_FlyForward"))
                plr.transform.position += plr.transform.forward / 5 * multiplier;

            if (HotkeyUtilities.IsHotkeyHeld("_FlyBackward"))
                plr.transform.position -= plr.transform.forward / 5 * multiplier;
        }

        public static void VehicleFlight()
        {
            InteractableVehicle vehicle = Player.player.movement.getVehicle();

            if (vehicle == null)
                return;

            Rigidbody rb = vehicle.GetComponent<Rigidbody>();

            if (rb == null)
                return;

            if (MiscOptions.VehicleFly)
            {
                float speedMul = MiscOptions.VehicleUseMaxSpeed ? vehicle.asset.TargetForwardVelocity * Time.fixedDeltaTime : MiscOptions.SpeedMultiplier / 3;

                rb.useGravity = false;
                rb.isKinematic = true;

                Transform tr = vehicle.transform;

                if (HotkeyUtilities.IsHotkeyHeld("_VFStrafeUp"))
                    tr.position = tr.position + new Vector3(0f, speedMul * 0.65f, 0f);

                if (HotkeyUtilities.IsHotkeyHeld("_VFStrafeDown"))
                    tr.position = tr.position - new Vector3(0f, speedMul * 0.65f, 0f);

                if (HotkeyUtilities.IsHotkeyHeld("_VFStrafeLeft"))
                    rb.MovePosition(tr.position - tr.right * speedMul);

                if (HotkeyUtilities.IsHotkeyHeld("_VFStrafeRight"))
                    rb.MovePosition(tr.position + tr.right * speedMul);

                if (HotkeyUtilities.IsHotkeyHeld("_VFMoveForward"))
                    rb.MovePosition(tr.position + tr.forward * speedMul);

                if (HotkeyUtilities.IsHotkeyHeld("_VFMoveBackward"))
                    rb.MovePosition(tr.position - tr.forward * speedMul);

                if (HotkeyUtilities.IsHotkeyHeld("_VFRotateRight"))
                    tr.Rotate(0f, 1f, 0f);

                if (HotkeyUtilities.IsHotkeyHeld("_VFRotateLeft"))
                    tr.Rotate(0f, -1f, 0f);

                if (HotkeyUtilities.IsHotkeyHeld("_VFRollLeft"))
                    tr.Rotate(0f, 0f, 2f);

                if (HotkeyUtilities.IsHotkeyHeld("_VFRollRight"))
                    tr.Rotate(0f, 0f, -2f);

                if (HotkeyUtilities.IsHotkeyHeld("_VFRotateUp"))
                    vehicle.transform.Rotate(-2f, 0f, 0f);

                if (HotkeyUtilities.IsHotkeyHeld("_VFRotateDown"))
                    vehicle.transform.Rotate(2f, 0f, 0f);
            }
            else
            {
                rb.useGravity = true;
                rb.isKinematic = false;
            }
        }

        public static void CheckMovementVerification() =>
            Instance.StartCoroutine(CheckVerification(Player.player.transform.position));

        public static IEnumerator CheckVerification(Vector3 LastPos)
        {
            if (Time.realtimeSinceStartup - LastMovementCheck < 0.8f)
                yield break;

            LastMovementCheck = Time.realtimeSinceStartup;
            Player.player.transform.position = new Vector3(0, -1337, 0);
            yield return new WaitForSeconds(3);

            //DebugUtilities.Log(VectorUtilities.GetDistance(Player.player.transform.position, LastPos));
            if (VectorUtilities.GetDistance(Player.player.transform.position, LastPos) < 10)
            {
                MiscOptions.NoMovementVerification = false;
            }
            else
            {
                MiscOptions.NoMovementVerification = true;
                Player.player.transform.position = LastPos + new Vector3(0, 5, 0);
            }
        }
    }
}