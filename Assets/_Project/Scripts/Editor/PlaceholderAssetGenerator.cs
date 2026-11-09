using System.IO;
using FarmVsForest.Core;
using FarmVsForest.Runtime;
using UnityEditor;
using UnityEngine;

namespace FarmVsForest.Editor
{
    /// <summary>
    /// Turns the code-defined placeholder cards into real assets you can edit in the inspector,
    /// plus each team's starting card and a starter deck per team. Safe to re-run: existing assets are left alone.
    /// </summary>
    public static class PlaceholderAssetGenerator
    {
        private const string Root = "Assets/_Project/Data";

        [MenuItem("Farm vs Forest/Generate Placeholder Card Assets")]
        public static void Generate()
        {
            int created = 0;
            foreach (Team team in System.Enum.GetValues(typeof(Team)))
            {
                var deckPath = Root + "/Decks/" + team + " Starter.asset";
                bool deckExists = AssetDatabase.LoadAssetAtPath<DeckAsset>(deckPath) != null;
                var deck = deckExists ? null : ScriptableObject.CreateInstance<DeckAsset>();
                if (deck != null) deck.team = team;

                CreateCard(Root + "/Cards/" + team, PlaceholderCards.StartingCard(team), ref created);

                foreach (var def in PlaceholderCards.ForTeam(team))
                {
                    var asset = CreateCard(Root + "/Cards/" + team, def, ref created);
                    if (deck != null)
                        deck.cards.Add(new DeckAsset.Entry { card = asset, copies = Copies(team, def) });
                }

                if (deck != null)
                {
                    EnsureFolder(Path.GetDirectoryName(deckPath));
                    AssetDatabase.CreateAsset(deck, deckPath);
                    created++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Farm vs Forest: created " + created + " placeholder assets under " + Root + ".");
        }

        /// <summary>Copies in the starter deck: designed teams have a deck list, placeholder teams use 12 free / 7 paid.</summary>
        private static int Copies(Team team, CardDefinition def)
        {
            if (team == Team.Forest)
                foreach (var (card, copies) in ForestCards.DeckList)
                    if (card.id == def.id) return copies;
            return def.IsFree ? 12 : 7;
        }

        private static CardDefinitionAsset CreateCard(string folder, CardDefinition def, ref int created)
        {
            string path = folder + "/" + def.displayName + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<CardDefinitionAsset>(path);
            if (existing != null) return existing;

            EnsureFolder(folder);
            var asset = ScriptableObject.CreateInstance<CardDefinitionAsset>();
            asset.card = def;
            AssetDatabase.CreateAsset(asset, path);
            created++;
            return asset;
        }

        private static void EnsureFolder(string folder)
        {
            folder = folder.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
