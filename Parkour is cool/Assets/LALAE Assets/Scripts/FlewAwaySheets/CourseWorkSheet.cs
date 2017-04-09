using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

namespace UnityStandardAssets._2D {



	public sealed class CourseWorkSheet:QuestObject {
		public override string Name {
			get {
				return "Sheet";
			}
		}

		public override string Info {
			get {
				return "Course work sheet";
			}
		}

		public override void Action(PlatformerCharacter2D player) {
			//player.findQuest(QuestName).Update(this);
		}

		public override string QuestName {
			get {
				return "Flew Away Sheets";
			}
		}

		void OnTriggerEnter2D(Collider2D other) {
			var player = other.transform.GetComponent<PlatformerCharacter2D>();
			if (player==null)
				return;
			var thisQuest = player.ActiveQuests.Find(quest => quest.Name==QuestName);
			if (thisQuest != null) {
				thisQuest.Update(this);
				Destroy(this.gameObject);
			}
		}

	}
}