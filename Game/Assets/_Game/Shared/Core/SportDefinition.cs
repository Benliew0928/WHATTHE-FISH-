using UnityEngine;
namespace SportsPrototype {
 [CreateAssetMenu(menuName="Sports/Sport Definition")]
 public sealed class SportDefinition:ScriptableObject {public SportId id;public string displayName;public bool available;public int maxPlayers=10;}
}
