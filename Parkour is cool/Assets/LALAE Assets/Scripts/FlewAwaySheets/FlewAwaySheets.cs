using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;


public sealed class FlewAwaySheets : Quest {
	private const int SheetNum = 6; //Нужно собрать 6 листов
	private int SheetCount = 0;     //Собрано сейчас

	public override string Name {
		get {
			return "Flew Away Sheets";
		}
	}
	public override string Info {
		get {
			return "Wind lalalalala catch them all. Pokemon!!";
		}
	}
	public override IReward Reward {
		get {
			return new ExpReward(20);
		}
	}

	protected override void Begin() {
		//Spawn 6 Sheets : QuestObject
	}
	protected override void End() {
	}
	public override void Update(QuestObject qo) {
		if (State==STATE.DONE && qo.Name=="Wind") {
			Finish();
			return;
		}
		if (qo.Name == "Sheet")
			SheetCount++;
		if (SheetCount>=SheetNum)
			_state=STATE.DONE;

	}
	public override string GetProgress() {
		var response = "Sheets collected: " + SheetCount + "\\" + SheetNum;
		if (State==STATE.DONE)
			response = "Return to Wind";
		return response;
	}
}
