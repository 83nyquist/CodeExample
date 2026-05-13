# Unity Testing Guidelines

## Rule Type
Always - Apply to all code generation

## Test Structure - FOLDER ORGANIZATION

Assets/
    Scripts/
        Gameplay/
            PlayerController.cs
    Tests/
        EditMode/
            Gameplay/
                PlayerControllerTests.cs
        PlayMode/
            Gameplay/
                PlayerIntegrationTests.cs

## Test Naming Convention

Format: [MethodName]_[Scenario]_[ExpectedResult]

Examples:
- TakeDamage_WhenDamageIsPositive_ReducesHealth()
- Jump_WhenGrounded_IncreasesVerticalVelocity()
- CollectItem_WhenInventoryFull_ReturnsFalse()

## EditMode Tests (No Scene Required) - TEMPLATE

using NUnit.Framework;
using UnityEngine;

public class PlayerControllerTests
{
    private PlayerController _player;
    
    [SetUp]
    public void Setup()
    {
        _player = new GameObject().AddComponent<PlayerController>();
    }
    
    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_player.gameObject);
    }
    
    [Test]
    public void TakeDamage_WhenDamageIsPositive_ReducesHealth()
    {
        // Arrange
        int initialHealth = 100;
        int damage = 25;
        _player.SetHealth(initialHealth);
        
        // Act
        _player.TakeDamage(damage);
        
        // Assert
        Assert.AreEqual(initialHealth - damage, _player.CurrentHealth);
    }
    
    [Test]
    public void TakeDamage_WhenDamageIsZero_DoesNotChangeHealth()
    {
        // Arrange
        int initialHealth = 100;
        _player.SetHealth(initialHealth);
        
        // Act
        _player.TakeDamage(0);
        
        // Assert
        Assert.AreEqual(initialHealth, _player.CurrentHealth);
    }
}

## PlayMode Tests (Requires Scene) - TEMPLATE

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class PlayerIntegrationTests
{
    [UnityTest]
    public IEnumerator PlayerMovement_WhenMovingRight_IncreasesXPosition()
    {
        // Arrange
        var player = GameObject.FindWithTag("Player");
        Vector3 startPosition = player.transform.position;
        
        // Act
        player.GetComponent<PlayerController>().Move(Vector2.right);
        yield return new WaitForFixedUpdate();
        
        // Assert
        Assert.Greater(player.transform.position.x, startPosition.x);
    }
}

## Required Tests for Every New Feature - CHECKLIST

- [ ] Happy path - Normal operation works
- [ ] Edge cases - Boundary values (0, null, empty, max)
- [ ] Error cases - Invalid inputs handled gracefully
- [ ] Performance - No GC allocation for Update/FixedUpdate code

## Required Tests for Bug Fixes - CHECKLIST

- [ ] Regression test - Proves the bug is fixed
- [ ] Edge case coverage - Tests the specific failing scenario

## Performance Test Example

[Test]
public void UpdateMethod_DoesNotAllocateMemory()
{
    var player = new GameObject().AddComponent<PlayerController>();
    Assert.That(() => player.Update(), Has.No.AllocatedGCMemory());
}

## Mocking Dependencies - USE INTERFACES

public interface IDamageable
{
    void TakeDamage(int damage);
}

public class PlayerController : MonoBehaviour, IDamageable
{
    public void TakeDamage(int damage) { }
}

// Test with mock
[Test]
public void EnemyAI_WhenCollidingWithPlayer_DealsDamage()
{
    var mockPlayer = Substitute.For<IDamageable>();
    var enemy = new GameObject().AddComponent<EnemyAI>();
    enemy.SetTarget(mockPlayer);
    
    enemy.DealDamage(25);
    
    mockPlayer.Received(1).TakeDamage(25);
}

## PR Requirements - CHECKLIST

- [ ] New features include EditMode tests
- [ ] Integration features include PlayMode tests
- [ ] Bug fixes include regression test
- [ ] All existing tests still pass
- [ ] No tests disabled with [Ignore] without documented reason
- [ ] Tests are deterministic (no random failures)

## Running Tests

Command line (CI/CD):
Unity -batchmode -runTests -testPlatform EditMode -testResults results.xml

In Rider: Right-click on test or test folder -> Run

## Common Mistakes to AVOID

- Tests that depend on execution order
- Tests that modify global state (PlayerPrefs, static variables)
- Tests that pass but never actually assert
- Tests that are too slow (avoid WaitForSeconds greater than 0.5f)
- Tests that require manual setup (must be automated)