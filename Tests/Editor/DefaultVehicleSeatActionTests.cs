using NUnit.Framework;

namespace MultiplayerARPG.Tests
{
    public partial class VehicleSeatActionTests
    {
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void DefaultControllerButtonsFollowSeatPermissions(bool canActivate, bool canPickup)
        {
            _vehicle.Seats[0].canActivate = canActivate;
            _vehicle.Seats[0].canPickup = canPickup;
            _character.SetPassengingVehicle(0, _vehicle);
            var controller = Child("Default controller").AddComponent<PlayerCharacterController>();
            var activate = Child("Activate detector").AddComponent<NearbyEntityDetector>();
            var pickup = Child("Pickup detector").AddComponent<NearbyEntityDetector>();
            Assert.That(controller, Is.Not.Null, "Default controller could not be created");
            Assert.That(activate, Is.Not.Null, "Activation detector could not be created");
            Assert.That(pickup, Is.Not.Null, "Pickup detector could not be created");
            Assert.That(pickup.pickupActivatableEntities, Is.Not.Null, "Pickup detector list was not initialized");
            activate.activatableEntities.Add(_target);
            activate.holdActivatableEntities.Add(_target);
            pickup.pickupActivatableEntities.Add(_target);
            typeof(PlayerCharacterController).GetProperty("ActivatableEntityDetector").SetValue(controller, activate);
            typeof(PlayerCharacterController).GetProperty("ItemDropEntityDetector").SetValue(controller, pickup);
            Assert.That(controller.ShouldShowActivateButtons(), Is.EqualTo(canActivate));
            Assert.That(controller.ShouldShowHoldActivateButtons(), Is.EqualTo(canActivate));
            Assert.That(controller.ShouldShowPickUpButtons(), Is.EqualTo(canPickup));
        }
    }
}
