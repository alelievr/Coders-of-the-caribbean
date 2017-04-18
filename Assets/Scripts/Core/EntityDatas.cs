using UnityEngine;

public class EntityData {

	public string	entityType;
	public int		entityId;
	public int		x;
	public int		y;

}

public class ShipData : EntityData {

	public int orientation;
	public int speed;
	public int health;
	public int owner;

	public ShipData(int x, int y, int entityId, int orientation, int speed, int health, int owner)
	{
		this.x = x;
		this.y = y;
		this.orientation = orientation;
		this.entityType = "SHIP";
		this.speed = speed;
		this.health = health;
		this.owner = owner;
		this.entityId = entityId;
	}

}

public class MineData : EntityData {
	
	public MineData(int x, int y, int entityId)
	{
		this.x = x;
		this.y = y;
		this.entityType = "MINE";
		this.entityId = entityId;
	}

}

public class CannonBallData : EntityData {
		
	public int		owner;
	public int		remainingTurns;

	public CannonBallData(int x, int y, int entityId, int owner, int remainingTurns)
	{
		this.x = x;
		this.y = y;
		this.entityType = "CANNONBALL";
		this.owner = owner;
		this.remainingTurns = remainingTurns;
		this.entityId = entityId;
	}

}

public class RumBarrelData : EntityData {

	public int	health;

	public RumBarrelData(int x, int y, int entityId, int health)
	{
		this.x = x;
		this.y = y;
		this.entityType = "BARREL";
		this.health = health;
		this.entityId = entityId;
	}

}