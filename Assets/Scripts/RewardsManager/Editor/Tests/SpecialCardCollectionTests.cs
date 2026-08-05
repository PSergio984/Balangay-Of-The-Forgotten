using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class SpecialCardCollectionTests
{
    private SpecialCardCollectionData _collection;
    private List<SpecialCardData> _testCards;

    [SetUp]
    public void SetUp()
    {
        _collection = ScriptableObject.CreateInstance<SpecialCardCollectionData>();
        _collection.Clear();

        _testCards = new List<SpecialCardData>();
        _testCards.Add(CreateTestCard("SPECIAL_TEST_1", "Test Card 1"));
        _testCards.Add(CreateTestCard("SPECIAL_TEST_2", "Test Card 2"));

        var listField = typeof(SpecialCardCollectionData).GetField(
            "allSpecialCardAssets",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        listField?.SetValue(_collection, _testCards);
    }

    [TearDown]
    public void TearDown()
    {
        if (_collection != null)
        {
            _collection.Clear();
            Object.DestroyImmediate(_collection);
        }
        foreach (var card in _testCards)
        {
            if (card != null)
            {
                Object.DestroyImmediate(card);
            }
        }
    }

    private static SpecialCardData CreateTestCard(string cardId, string cardName)
    {
        var card = ScriptableObject.CreateInstance<SpecialCardData>();
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(SpecialCardData).GetField("cardId", flags)?.SetValue(card, cardId);
        typeof(SpecialCardData).GetField("cardName", flags)?.SetValue(card, cardName);
        return card;
    }

    [Test]
    public void AddSpecialCardToRandomHero_AssignsToEligibleHero()
    {
        bool added = _collection.AddSpecialCardToRandomHero(_testCards[0], new List<string> { "Lakan" });

        Assert.IsTrue(added);
        Assert.IsTrue(_collection.HasCard("SPECIAL_TEST_1"));
        Assert.IsNotNull(_collection.GetSpecialCardForHero("Lakan"));
    }

    [Test]
    public void AddSpecialCardToRandomHero_NoActiveHeroes_ReturnsFalse()
    {
        bool added = _collection.AddSpecialCardToRandomHero(_testCards[0], new List<string>());

        Assert.IsFalse(added);
    }

    [Test]
    public void SaveAndLoad_RoundTripsAssignments()
    {
        _collection.AddSpecialCardToRandomHero(_testCards[0], new List<string> { "Lakan" });

        var freshInstance = ScriptableObject.CreateInstance<SpecialCardCollectionData>();
        var listField = typeof(SpecialCardCollectionData).GetField(
            "allSpecialCardAssets",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        listField?.SetValue(freshInstance, _testCards);
        freshInstance.Load();

        Assert.IsTrue(freshInstance.HasCard("SPECIAL_TEST_1"));
        Assert.IsNotNull(freshInstance.GetSpecialCardForHero("Lakan"));
        Assert.AreEqual("Test Card 1", freshInstance.GetSpecialCardForHero("Lakan").CardName);

        Object.DestroyImmediate(freshInstance);
    }

    [Test]
    public void ConsumeSpecialCardForHero_RemovesAssignmentAndCard()
    {
        _collection.AddSpecialCardToRandomHero(_testCards[0], new List<string> { "Lakan" });

        bool consumed = _collection.ConsumeSpecialCardForHero("Lakan", _testCards[0]);

        Assert.IsTrue(consumed);
        Assert.IsFalse(_collection.HasCard("SPECIAL_TEST_1"));
        Assert.IsNull(_collection.GetSpecialCardForHero("Lakan"));
    }

    [Test]
    public void ConsumeSpecialCardForHero_UnassignedHero_ReturnsFalse()
    {
        bool consumed = _collection.ConsumeSpecialCardForHero("Lakan", _testCards[0]);

        Assert.IsFalse(consumed);
    }

    [Test]
    public void AddSpecialCardToRandomHero_AllowsStackingUpToMaxCapacityOnOneHero()
    {
        _collection.AddSpecialCardToRandomHero(_testCards[0], new List<string> { "Lakan" });

        bool added = _collection.AddSpecialCardToRandomHero(_testCards[1], new List<string> { "Lakan" });

        Assert.IsTrue(added);
        Assert.AreEqual(2, _collection.GetSpecialCardsForHero("Lakan").Count);
    }

    [Test]
    public void AddSpecialCardToRandomHero_BlocksFourthCardOnSameHero()
    {
        List<string> heroNames = new List<string> { "HeroA" };
        for (int i = 0; i < 3; i++)
        {
            bool added = _collection.AddSpecialCardToRandomHero(CreateTestCard("CAP_" + i, "Cap " + i), heroNames);
            Assert.IsTrue(added);
        }

        bool extra = _collection.AddSpecialCardToRandomHero(_testCards[0], heroNames);
        Assert.IsFalse(extra);
    }

    [Test]
    public void AddSpecialCardToRandomHero_FullHeroRedirectsToAnotherHero()
    {
        for (int i = 0; i < 3; i++)
        {
            bool added = _collection.AddSpecialCardToRandomHero(CreateTestCard("RED_" + i, "Red " + i), new List<string> { "HeroA" });
            Assert.IsTrue(added);
        }

        bool redirected = _collection.AddSpecialCardToRandomHero(_testCards[0], new List<string> { "HeroA", "HeroB" });

        Assert.IsTrue(redirected);
        Assert.AreEqual(1, _collection.GetSpecialCardsForHero("HeroB").Count);
    }

    [Test]
    public void AddSpecialCardToRandomHero_RejectsCardAlreadyAssignedToAnotherHero()
    {
        _collection.AddSpecialCardToRandomHero(_testCards[0], new List<string> { "Lakan" });

        bool again = _collection.AddSpecialCardToRandomHero(_testCards[0], new List<string> { "Datu" });

        Assert.IsFalse(again);
    }

    [Test]
    public void GetSpecialCardsForHero_ReturnsAllStackedCards()
    {
        _collection.AddSpecialCardToRandomHero(_testCards[0], new List<string> { "Lakan" });
        _collection.AddSpecialCardToRandomHero(_testCards[1], new List<string> { "Lakan" });

        var cards = _collection.GetSpecialCardsForHero("Lakan");

        Assert.AreEqual(2, cards.Count);
    }

    [Test]
    public void GetSpecialCardsForHero_UnknownHero_ReturnsEmpty()
    {
        var cards = _collection.GetSpecialCardsForHero("Nobody");

        Assert.IsNotNull(cards);
        Assert.AreEqual(0, cards.Count);
    }

    [Test]
    public void ConsumeSpecialCardForHero_RemovesOnlyMatchingCardFromStack()
    {
        _collection.AddSpecialCardToRandomHero(_testCards[0], new List<string> { "Lakan" });
        _collection.AddSpecialCardToRandomHero(_testCards[1], new List<string> { "Lakan" });

        bool consumed = _collection.ConsumeSpecialCardForHero("Lakan", _testCards[0]);

        Assert.IsTrue(consumed);
        Assert.IsFalse(_collection.HasCard("SPECIAL_TEST_1"));
        Assert.IsTrue(_collection.HasCard("SPECIAL_TEST_2"));
        var remaining = _collection.GetSpecialCardsForHero("Lakan");
        Assert.AreEqual(1, remaining.Count);
        Assert.AreEqual("Test Card 2", remaining[0].CardName);
    }

    [Test]
    public void SaveAndLoad_RoundTripsStackedAssignments()
    {
        _collection.AddSpecialCardToRandomHero(_testCards[0], new List<string> { "Lakan" });
        _collection.AddSpecialCardToRandomHero(_testCards[1], new List<string> { "Lakan" });

        var freshInstance = ScriptableObject.CreateInstance<SpecialCardCollectionData>();
        var listField = typeof(SpecialCardCollectionData).GetField(
            "allSpecialCardAssets",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        listField?.SetValue(freshInstance, _testCards);
        freshInstance.Load();

        var restored = freshInstance.GetSpecialCardsForHero("Lakan");
        Assert.AreEqual(2, restored.Count);

        Object.DestroyImmediate(freshInstance);
    }

    [Test]
    public void IsSpecialCardData_MatchesRuntimePlayableDataOfAssignedCard()
    {
        _collection.AddSpecialCardToRandomHero(_testCards[0], new List<string> { "Lakan" });
        CardData playable = _testCards[0].GetOrCreatePlayableCardData();

        bool result = _collection.IsSpecialCardData(playable);

        Assert.IsTrue(result);
    }

    [Test]
    public void IsSpecialCardData_MatchesCatalogCardEvenWhenConsumed()
    {
        _collection.AddSpecialCardToRandomHero(_testCards[0], new List<string> { "Lakan" });
        CardData playable = _testCards[0].GetOrCreatePlayableCardData();
        _collection.ConsumeSpecialCardForHero("Lakan", _testCards[0]);

        bool result = _collection.IsSpecialCardData(playable);

        Assert.IsTrue(result);
    }

    [Test]
    public void IsSpecialCardData_CardFromAnotherCatalog_ReturnsFalse()
    {
        CardData unrelated = ScriptableObject.CreateInstance<CardData>();

        bool result = _collection.IsSpecialCardData(unrelated);

        Assert.IsFalse(result);
        Object.DestroyImmediate(unrelated);
    }

    [Test]
    public void Clear_RemovesAllCardsAndAssignments()
    {
        _collection.AddSpecialCardToRandomHero(_testCards[0], new List<string> { "Lakan" });

        _collection.Clear();

        Assert.AreEqual(0, _collection.CardCount);
        Assert.IsFalse(_collection.HasAvailableCards);
        Assert.IsNull(_collection.GetSpecialCardForHero("Lakan"));
    }

    [Test]
    public void GetOrCreatePlayableCardData_WithNullRepresentation_IncludesSpecialCardEffect()
    {
        var card = CreateTestCard("DYNAMIC_1", "Dynamic Card");
        var effectField = typeof(SpecialCardData).GetField(
            "cardDataRepresentation",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        effectField?.SetValue(card, null);

        CardData playable = card.GetOrCreatePlayableCardData();

        Assert.IsNotNull(playable);
        Assert.IsNotNull(playable.OtherEffects);
        Assert.Greater(playable.OtherEffects.Count, 0);
        bool hasSpecialEffect = false;
        foreach (var wrapper in playable.OtherEffects)
        {
            if (wrapper != null && wrapper.effects is SpecialCardEffect)
            {
                hasSpecialEffect = true;
                Assert.AreEqual(card, ((SpecialCardEffect)wrapper.effects).SpecialCardData);
            }
        }
        Assert.IsTrue(hasSpecialEffect, "Playable card should include a SpecialCardEffect");
    }
}