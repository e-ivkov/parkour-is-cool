using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

public class QuestWind : QuestObject, QuestGiver {
	public void GiveQuest(PlatformerCharacter2D player) {
		new FlewAwaySheets().Accept(player);
	}

	public override string Name {
		get {
			return "Wind"; }
	}

	public override string Info {
		get { return ""; }
	}

	public override void Action(PlatformerCharacter2D player) {
		;
	}

	public override string QuestName {
		get {
			return "Flew Away Sheets"; }
	}

	bool questDone = false;

	void OnTriggerEnter2D(Collider2D other) {
		var player = other.transform.GetComponent<PlatformerCharacter2D>();
		if (player==null)
			return;
		var quest = player.FindQuest(player.ActiveQuests, QuestName);
		if (questDone) {
			dialog.Text.text="Thanks!";
			dialog.Show(1);
		} else if (quest==null) {
			dialog.Text.text="Hello my friend, lalala, catch them all. lalala!";
			dialog.Show(3);
			GiveQuest(player);
		} else if (quest.State==Quest.STATE.DONE) {
			questDone=true;
			dialog.Text.text="Wow! Thank you!";
			dialog.Show(3);
			quest.Update(this);
			//Destroy(this.gameObject);
		}
	}
}
