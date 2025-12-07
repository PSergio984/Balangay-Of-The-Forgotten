using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/* ENEMY MOVE DATA DOCUMENTATION
 *
 * Purpose: ScriptableObject that defines the design-time properties and data for enemy moves
 *
 * How it works:
 * - Stores all move information like name, and effects
 * - Can be created as assets in the project for different enemy moves
 * - Supports both manual targeting effects and automatic targeting effects
 * - Used as templates to create runtime enemy move instances during gameplay
 *
 * Integration: Used by Enemy model and EnemySystem to create and execute moves
 */

/// <summary>
/// ScriptableObject asset that defines an enemy move's properties, effects, and behavior
/// </summary>
[CreateAssetMenu(menuName = "Data/EnemyMove")]
public class EnemyMoveData : ScriptableObject
{
	[Title("Move Information", "Basic properties of this enemy move", TitleAlignments.Centered)]
	[BoxGroup("Basic Info")]
	/// <summary>
	/// The name and description text displayed for the move
	/// </summary>
	/// <remarks>
	/// This property holds the move's title and description that designers see.
	/// Set this in the Inspector to define what the move is called and what it does.
	/// </remarks>
	[field: SerializeField]
	[field: BoxGroup("Basic Info")]
	[field: LabelText("Move Description")]
	[field: MultiLineProperty(3)]
	[field: Required("Move must have a description!")]
	[field: ValidateInput("@!string.IsNullOrWhiteSpace($value)", "Description cannot be empty or whitespace")]
	public string Description { get; private set; }

	/// <summary>
	/// Additional information text displayed for the move
	/// </summary>
	/// <remarks>
	/// This property holds detailed information about the move's mechanics, lore, or usage tips.
	/// Complements the Description field by providing extra context or flavor text.
	/// </remarks>
	[field: SerializeField]
	[field: BoxGroup("Basic Info")]
	[field: LabelText("Move Information")]
	[field: MultiLineProperty(3)]
	[field: Required("Move must have information!")]
	[field: ValidateInput("@!string.IsNullOrWhiteSpace($value)", "Information cannot be empty or whitespace")]
	public string Information { get; private set; }


	[Title("VFX & SFX", "Visual and audio effects for this move", TitleAlignments.Centered)]
	
	/// <summary>
	/// Optional VFX/SFX data for this move's visual and audio effects
	/// </summary>
	[field: SerializeField]
	[field: LabelText("Move VFX/SFX")]
	[field: InfoBox("Optional: Assign custom VFX and SFX for this move", InfoMessageType.None)]
	public CardVFXData VFXData { get; private set; }

	[Title("Move Effects", "Define what this move does when used", TitleAlignments.Centered)]
	[InfoBox("Manual Target Effect: AI chooses the target (like single-target damage)\nOther Effects: Automatic targeting (like area damage, self-buffs)", InfoMessageType.Info)]

	[InfoBox("@GetMoveValidationMessage()", InfoMessageType.Warning, "HasMoveValidationIssues")]

	/// <summary>
	/// Single effect that requires manual target selection by the AI
	/// </summary>
	/// <remarks>
	/// Holds an effect where the AI must choose the target. Examples: single-target damage, targeted heal, specific hero debuff.
	/// Can be null if the move doesn't have any manual targeting effects.
	/// Set this in the Inspector for moves that need target selection.
	/// </remarks>
	[field: SerializeReference]
	[field: ShowInInspector]
	[field: LabelText("Main Effect (Manual Target)")]
	[field: InfoBox("This effect requires the AI to choose a target", InfoMessageType.None, "@ManualTargetEffect != null")]
	public Effects ManualTargetEffect { get; private set; } = null;

	/// <summary>
	/// List of effects that automatically select their own targets
	/// </summary>
	/// <remarks>
	/// Contains effects that don't need AI input for targeting. Each effect has its own target mode (all heroes, random target, self, etc.).
	/// Moves can have multiple automatic effects that trigger when the move is used. Examples: area damage + self-heal, enemy debuff + buff, etc.
	/// Set these in the Inspector for moves with automatic or multiple effects.
	/// </remarks>
	[field: SerializeField]
	[field: LabelText("Secondary Effects (Auto-Target)")]
	[field: ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
	public List<AutoTargetEffect> OtherEffects { get; private set; }

	// Validation methods for better debugging
	private bool HasMoveValidationIssues()
	{
		return ManualTargetEffect == null && (OtherEffects == null || OtherEffects.Count == 0);
	}

	private string GetMoveValidationMessage()
	{
		if (ManualTargetEffect == null && (OtherEffects == null || OtherEffects.Count == 0))
			return "⚠️ This move has no effects! Add either a Manual Target Effect or Other Effects.";
		return "";
	}
}
