using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityStandardAssets._2D;

public interface IReward{
	void Accept(PlatformerCharacter2D player);
}

public class ExpReward : IReward {
	private int xp;
	public ExpReward(int xp) {
		this.xp=xp;
	}
	public void Accept(PlatformerCharacter2D player) {
		player.Exp+=xp;
	}
}


public abstract class QuestObject:MonoBehaviour {
	public Dialog dialog;
	public abstract string Name { get; }
	public abstract string Info { get; }
	
	public abstract void Action(PlatformerCharacter2D player);
	
	public abstract string QuestName { get; }

	void OnTriggerEnter2D(Collider2D other) {
		var thisQuest = other.transform.GetComponent<PlatformerCharacter2D>().ActiveQuests.Find(quest => quest.Name==QuestName);
		if (thisQuest != null)
			thisQuest.Update(this);
	}
}


public interface QuestGiver {
	void GiveQuest(PlatformerCharacter2D player);
}


public abstract class Quest{
	public enum STATE {DONE, IN_PROGRESS, NOT_ACCEPTED, ABORTED, FINISHED}    //Состояние квестов 
	
	protected STATE _state = STATE.NOT_ACCEPTED;    //Состояние квеста
	protected PlatformerCharacter2D Player;         //Игрок, взявший квест
	protected abstract void Begin();                //инициализация квеста (спавн итемов)
	protected abstract void End();                  //удаление всего связанного с квестом (доп. условия)

	//Interface

	public abstract string Name {get;}					//Название квеста  
	public abstract string Info {get;}					//Описание квеста
	public abstract IReward Reward {get;}				//Награда за квест
	public STATE State {get {return _state;}}			//Возвращает состояние квеста

	public abstract void Update(QuestObject qo);		//обновление инфы о квесты (квестовые итемы вызывают этот метод, полуая квест из персонажа)
	public abstract string GetProgress();				//возвращает инф. о квесте (собрано 4/6 из листов)

	public void Accept(PlatformerCharacter2D player) {  //Принятие квеста
		if (State != STATE.NOT_ACCEPTED)
			return;
		Player=player;
		Player.AddQuest(this);
		Begin();
		_state=STATE.IN_PROGRESS;
	}
	public void Finish() {                              //Сдача\завершение квеста
		if (State != STATE.DONE)
			return;
		Player.FinishQuest(this);
		End();
		_state=STATE.FINISHED;
	}
	public void Abort() {                               //Отмена квеста (сдача без награды, провал)
		if (State != STATE.IN_PROGRESS)
			return;
		Player.AbortQuest(this);
		_state=STATE.ABORTED;
	}

}
