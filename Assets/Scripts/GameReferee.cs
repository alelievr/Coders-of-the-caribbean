using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class Properties : Dictionary< string, object >
{
    public void put(string k, object o)
    {
        base[k] = o;
    }

    public void setProperty(string k, object o)
    {
        base[k] = o;
    }

    public object getProperty(string k, string defaultValue = null)
    {
        if (ContainsKey(k))
            return base[k];
        else
            return defaultValue;
    }
}

class GameReferee {

    private static readonly int LEAGUE_LEVEL = 3;

    private static int MAP_WIDTH = 23;
    private static int MAP_HEIGHT = 21;
    private static int COOLDOWN_CANNON = 2;
    private static int COOLDOWN_MINE = 5;
    private static int INITIAL_SHIP_HEALTH = 100;
    private static int MAX_SHIP_HEALTH = 100;
    private static int MAX_SHIP_SPEED;
    private static int MIN_SHIPS = 1;
    private static int MAX_SHIPS;
    private static int MIN_MINES;
    private static int MAX_MINES;
    private static int MIN_RUM_BARRELS = 10;
    private static int MAX_RUM_BARRELS = 26;
    private static int MIN_RUM_BARREL_VALUE = 10;
    private static int MAX_RUM_BARREL_VALUE = 20;
    private static int REWARD_RUM_BARREL_VALUE = 30;
    private static int MINE_VISIBILITY_RANGE = 5;
    private static int FIRE_DISTANCE_MAX = 10;
    private static int LOW_DAMAGE = 25;
    private static int HIGH_DAMAGE = 50;
    private static int MINE_DAMAGE = 25;
    private static int NEAR_MINE_DAMAGE = 10;
    private static bool CANNONS_ENABLED;
    private static bool MINES_ENABLED;

    public GameReferee()
    {
        switch (LEAGUE_LEVEL) {
        case 0: // 1 ship / no mines / speed 1
            MAX_SHIPS = 1;
            CANNONS_ENABLED = false;
            MINES_ENABLED = false;
            MIN_MINES = 0;
            MAX_MINES = 0;
            MAX_SHIP_SPEED = 1;
            break;
        case 1: // add mines
            MAX_SHIPS = 1;
            CANNONS_ENABLED = true;
            MINES_ENABLED = true;
            MIN_MINES = 5;
            MAX_MINES = 10;
            MAX_SHIP_SPEED = 1;
            break;
        case 2: // 3 ships max
            MAX_SHIPS = 3;
            CANNONS_ENABLED = true;
            MINES_ENABLED = true;
            MIN_MINES = 5;
            MAX_MINES = 10;
            MAX_SHIP_SPEED = 1;
            break;
        default: // increase max speed
            MAX_SHIPS = 3;
            CANNONS_ENABLED = true;
            MINES_ENABLED = true;
            MIN_MINES = 5;
            MAX_MINES = 10;
            MAX_SHIP_SPEED = 2;
            break;
        }
    }

    private static  Regex PLAYER_INPUT_MOVE_PATTERN = new Regex("MOVE (?<x>[0-9]{1,8})\\s+(?<y>[0-9]{1,8})(?:\\s+(?<message>.+))?", RegexOptions.IgnoreCase);
    private static  Regex PLAYER_INPUT_SLOWER_PATTERN = new Regex("SLOWER(?:\\s+(?<message>.+))?", RegexOptions.IgnoreCase);
    private static  Regex PLAYER_INPUT_FASTER_PATTERN = new Regex("FASTER(?:\\s+(?<message>.+))?", RegexOptions.IgnoreCase);
    private static  Regex PLAYER_INPUT_WAIT_PATTERN = new Regex("WAIT(?:\\s+(?<message>.+))?", RegexOptions.IgnoreCase);
    private static  Regex PLAYER_INPUT_PORT_PATTERN = new Regex("PORT(?:\\s+(?<message>.+))?", RegexOptions.IgnoreCase);
    private static  Regex PLAYER_INPUT_STARBOARD_PATTERN = new Regex("STARBOARD(?:\\s+(?<message>.+))?", RegexOptions.IgnoreCase);
    private static  Regex PLAYER_INPUT_FIRE_PATTERN = new Regex("FIRE (?<x>[0-9]{1,8})\\s+(?<y>[0-9]{1,8})(?:\\s+(?<message>.+))?", RegexOptions.IgnoreCase);
    private static  Regex PLAYER_INPUT_MINE_PATTERN = new Regex("MINE(?:\\s+(?<message>.+))?", RegexOptions.IgnoreCase);

    public static int clamp(int val, int min, int max) {
        return Mathf.Max(min, Mathf.Min(max, val));
    }

    public static string join(params object[] objs)
    {
        string      ret = "";

        foreach (var obj in objs)
            if (obj != objs.Last())
                ret += obj + " ";
            else
                ret += obj.ToString();
        return ret;
    }

    public class Coord {
        private  static int[,] DIRECTIONS_EVEN = new int[,] { { 1, 0 }, { 0, -1 }, { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, 1 } };
        private  static int[,] DIRECTIONS_ODD = new int[,] { { 1, 0 }, { 1, -1 }, { 0, -1 }, { -1, 0 }, { 0, 1 }, { 1, 1 } };
        public  int x;
        public  int y;

        public Coord(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public Coord(Coord other) {
            this.x = other.x;
            this.y = other.y;
        }

        public float angle(Coord targetPosition) {
            float dy = (targetPosition.y - this.y) * Mathf.Sqrt(3) / 2;
            float dx = targetPosition.x - this.x + ((this.y - targetPosition.y) & 1) * 0.5f;
            float angle = -Mathf.Atan2(dy, dx) * 3 / Mathf.PI;
            if (angle < 0) {
                angle += 6;
            } else if (angle >= 6) {
                angle -= 6;
            }
            return angle;
        }

        public CubeCoordinate toCubeCoordinate() {
            int xp = x - (y - (y & 1)) / 2;
            int zp = y;
            int yp = -(xp + zp);
            return new CubeCoordinate(xp, yp, zp);
        }

        public Coord neighbor(int orientation) {
            int newY, newX;
            if (this.y % 2 == 1) {
                newY = this.y + DIRECTIONS_ODD[orientation, 1];
                newX = this.x + DIRECTIONS_ODD[orientation, 0];
            } else {
                newY = this.y + DIRECTIONS_EVEN[orientation, 1];
                newX = this.x + DIRECTIONS_EVEN[orientation, 0];
            }

            return new Coord(newX, newY);
        }

        public bool isInsideMap() {
            return x >= 0 && x < MAP_WIDTH && y >= 0 && y < MAP_HEIGHT;
        }

        public int distanceTo(Coord dst) {
            return this.toCubeCoordinate().distanceTo(dst.toCubeCoordinate());
        }


        public bool equals(object obj) {
            if (obj == null || GetHashCode() != obj.GetHashCode()) {
                return false;
            }
            Coord other = (Coord) obj;
            return y == other.y && x == other.x;
        }


        public String toString() {
            return join(x, y);
        }
    }

    public class CubeCoordinate {
        static int[,] directions = new int[,] { { 1, -1, 0 }, { +1, 0, -1 }, { 0, +1, -1 }, { -1, +1, 0 }, { -1, 0, +1 }, { 0, -1, +1 } };
        int x, y, z;

        public CubeCoordinate(int x, int y, int z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Coord toOffsetCoordinate() {
            int newX = x + (z - (z & 1)) / 2;
            int newY = z;
            return new Coord(newX, newY);
        }

        public CubeCoordinate neighbor(int orientation) {
            int nx = this.x + directions[orientation, 0];
            int ny = this.y + directions[orientation, 1];
            int nz = this.z + directions[orientation, 2];

            return new CubeCoordinate(nx, ny, nz);
        }

        public int distanceTo(CubeCoordinate dst) {
            return (Mathf.Abs(x - dst.x) + Mathf.Abs(y - dst.y) + Mathf.Abs(z - dst.z)) / 2;
        }

        public String toString() {
            return join(x, y, z);
        }
    }

    public enum EntityType {
        SHIP, BARREL, MINE, CANNONBALL
    }

    public abstract class Entity {
        private static int UNIQUE_ENTITY_ID = 0;

        public  int id;
        public  EntityType type;
        public Coord position;

        public Entity(EntityType type, int x, int y) {
            this.id = UNIQUE_ENTITY_ID++;
            this.type = type;
            this.position = new Coord(x, y);
        }

        public String toViewString() {
            return join(id, position.y, position.x);
        }

        public String toPlayerString(int arg1, int arg2, int arg3, int arg4) {
            return join(id, type.ToString(), position.x, position.y, arg1, arg2, arg3, arg4);
        }
    }

    public class Mine : Entity {
        public Mine(int x, int y) : base(EntityType.MINE, x, y) {
        }

        public String toPlayerString(int playerIdx) {
            return toPlayerString(0, 0, 0, 0);
        }

        public List<Damage> explode(List<Ship> ships, bool force) {
            List<Damage> damage = new List< Damage >();
            Ship victim = null;

            foreach (Ship ship in ships) {
                if (position.equals(ship.bow()) || position.equals(ship.stern()) || position.equals(ship.position)) {
                    damage.Add(new Damage(this.position, MINE_DAMAGE, true));
                    ship.damage(MINE_DAMAGE);
                    victim = ship;
                }
            }

            if (force || victim != null) {
                if (victim == null) {
                    damage.Add(new Damage(this.position, MINE_DAMAGE, true));
                }

                foreach (Ship ship in ships) {
                    if (ship != victim) {
                        Coord impactPosition = null;
                        if (ship.stern().distanceTo(position) <= 1) {
                            impactPosition = ship.stern();
                        }
                        if (ship.bow().distanceTo(position) <= 1) {
                            impactPosition = ship.bow();
                        }
                        if (ship.position.distanceTo(position) <= 1) {
                            impactPosition = ship.position;
                        }

                        if (impactPosition != null) {
                            ship.damage(NEAR_MINE_DAMAGE);
                            damage.Add(new Damage(impactPosition, NEAR_MINE_DAMAGE, true));
                        }
                    }
                }
            }

            return damage;
        }
    }

    public class Cannonball : Entity {
        public int ownerEntityId;
        public int srcX;
        public int srcY;
        public int initialRemainingTurns;
        public int remainingTurns;

        public Cannonball(int row, int col, int ownerEntityId, int srcX, int srcY, int remainingTurns) : base(EntityType.CANNONBALL, row, col) {
            this.ownerEntityId = ownerEntityId;
            this.srcX = srcX;
            this.srcY = srcY;
            this.initialRemainingTurns = this.remainingTurns = remainingTurns;
        }

        public new String toViewString() {
            return join(id, position.y, position.x, srcY, srcX, initialRemainingTurns, remainingTurns, ownerEntityId);
        }

        public String toPlayerString(int playerIdx) {
            return toPlayerString(ownerEntityId, remainingTurns, 0, 0);
        }
    }

    public class RumBarrel : Entity {
        public int health;

        public RumBarrel(int x, int y, int health) : base(EntityType.BARREL, x, y) {
            this.health = health;
        }

        public new String toViewString() {
            return join(id, position.y, position.x, health);
        }

        public String toPlayerString(int playerIdx) {
            return toPlayerString(health, 0, 0, 0);
        }
    }

    public class Damage {
        public  Coord position;
        public  int health;
        public  bool hit;

        public Damage(Coord position, int health, bool hit) {
            this.position = position;
            this.health = health;
            this.hit = hit;
        }

        public String toViewString() {
            return join(position.y, position.x, health, (hit ? 1 : 0));
        }
    }

    public enum Action {
        FASTER, SLOWER, PORT, STARBOARD, FIRE, MINE
    }

    public class Ship : Entity {
        public int orientation;
        public int speed;
        public int health;
        public int owner;
        public String message;
        public Action? action;
        public int mineCooldown;
        public int cannonCooldown;
        public Coord target;
        public int newOrientation;
        public Coord newPosition;
        public Coord newBowCoordinate;
        public Coord newSternCoordinate;

        public Ship(int x, int y, int orientation, int owner) : base(EntityType.SHIP, x, y) {
            this.orientation = orientation;
            this.speed = 0;
            this.health = INITIAL_SHIP_HEALTH;
            this.owner = owner;
        }

        public new String toViewString() {
            return join(id, position.y, position.x, orientation, health, speed, (action != null ? action.ToString() : "WAIT"), bow().y, bow().x, stern().y,
                    stern().x, " ;" + (message != null ? message : ""));
        }

        public String toPlayerString(int playerIdx) {
            return toPlayerString(orientation, speed, health, owner == playerIdx ? 1 : 0);
        }

        public void setMessage(String message) {
            if (message != null && message.Length > 50) {
                message = message.Substring(0, 50) + "...";
            }
            this.message = message;
        }

        public void moveTo(int x, int y) {
            Coord currentPosition = this.position;
            Coord targetPosition = new Coord(x, y);

            if (currentPosition.equals(targetPosition)) {
                this.action = Action.SLOWER;
                return;
            }

            float targetAngle, angleStraight, anglePort, angleStarboard, centerAngle, anglePortCenter, angleStarboardCenter;

            switch (speed) {
            case 2:
                this.action = Action.SLOWER;
                break;
            case 1:
                // Suppose we've moved first
                currentPosition = currentPosition.neighbor(orientation);
                if (!currentPosition.isInsideMap()) {
                    this.action = Action.SLOWER;
                    break;
                }

                // Target reached at next turn
                if (currentPosition.equals(targetPosition)) {
                    this.action = null;
                    break;
                }

                // For each neighbor cell, find the closest to target
                targetAngle = currentPosition.angle(targetPosition);
                angleStraight = Mathf.Min(Mathf.Abs(orientation - targetAngle), 6 - Mathf.Abs(orientation - targetAngle));
                anglePort = Mathf.Min(Mathf.Abs((orientation + 1) - targetAngle), Mathf.Abs((orientation - 5) - targetAngle));
                angleStarboard = Mathf.Min(Mathf.Abs((orientation + 5) - targetAngle), Mathf.Abs((orientation - 1) - targetAngle));

                centerAngle = currentPosition.angle(new Coord(MAP_WIDTH / 2, MAP_HEIGHT / 2));
                anglePortCenter = Mathf.Min(Mathf.Abs((orientation + 1) - centerAngle), Mathf.Abs((orientation - 5) - centerAngle));
                angleStarboardCenter = Mathf.Min(Mathf.Abs((orientation + 5) - centerAngle), Mathf.Abs((orientation - 1) - centerAngle));

                // Next to target with bad angle, slow down then rotate (avoid to turn around the target!)
                if (currentPosition.distanceTo(targetPosition) == 1 && angleStraight > 1.5) {
                    this.action = Action.SLOWER;
                    break;
                }

                int? distanceMin = null;

                // Test forward
                Coord nextPosition = currentPosition.neighbor(orientation);
                if (nextPosition.isInsideMap()) {
                    distanceMin = nextPosition.distanceTo(targetPosition);
                    this.action = null;
                }

                // Test port
                nextPosition = currentPosition.neighbor((orientation + 1) % 6);
                if (nextPosition.isInsideMap()) {
                    int distance = nextPosition.distanceTo(targetPosition);
                    if (distanceMin == null || distance < distanceMin || distance == distanceMin && anglePort < angleStraight - 0.5) {
                        distanceMin = distance;
                        this.action = Action.PORT;
                    }
                }

                // Test starboard
                nextPosition = currentPosition.neighbor((orientation + 5) % 6);
                if (nextPosition.isInsideMap()) {
                    int distance = nextPosition.distanceTo(targetPosition);
                    if (distanceMin == null || distance < distanceMin
                            || (distance == distanceMin && angleStarboard < anglePort - 0.5 && this.action == Action.PORT)
                            || (distance == distanceMin && angleStarboard < angleStraight - 0.5 && this.action == null)
                            || (distance == distanceMin && this.action == Action.PORT && angleStarboard == anglePort
                                    && angleStarboardCenter < anglePortCenter)
                            || (distance == distanceMin && this.action == Action.PORT && angleStarboard == anglePort
                                    && angleStarboardCenter == anglePortCenter && (orientation == 1 || orientation == 4))) {
                        distanceMin = distance;
                        this.action = Action.STARBOARD;
                    }
                }
                break;
            case 0:
                // Rotate ship towards target
                targetAngle = currentPosition.angle(targetPosition);
                angleStraight = Mathf.Min(Mathf.Abs(orientation - targetAngle), 6 - Mathf.Abs(orientation - targetAngle));
                anglePort = Mathf.Min(Mathf.Abs((orientation + 1) - targetAngle), Mathf.Abs((orientation - 5) - targetAngle));
                angleStarboard = Mathf.Min(Mathf.Abs((orientation + 5) - targetAngle), Mathf.Abs((orientation - 1) - targetAngle));

                centerAngle = currentPosition.angle(new Coord(MAP_WIDTH / 2, MAP_HEIGHT / 2));
                anglePortCenter = Mathf.Min(Mathf.Abs((orientation + 1) - centerAngle), Mathf.Abs((orientation - 5) - centerAngle));
                angleStarboardCenter = Mathf.Min(Mathf.Abs((orientation + 5) - centerAngle), Mathf.Abs((orientation - 1) - centerAngle));

                Coord forwardPosition = currentPosition.neighbor(orientation);

                this.action = null;

                if (anglePort <= angleStarboard) {
                    this.action = Action.PORT;
                }

                if (angleStarboard < anglePort || angleStarboard == anglePort && angleStarboardCenter < anglePortCenter
                        || angleStarboard == anglePort && angleStarboardCenter == anglePortCenter && (orientation == 1 || orientation == 4)) {
                    this.action = Action.STARBOARD;
                }

                if (forwardPosition.isInsideMap() && angleStraight <= anglePort && angleStraight <= angleStarboard) {
                    this.action = Action.FASTER;
                }
                break;
            }

        }

        public void faster() {
            this.action = Action.FASTER;
        }

        public void slower() {
            this.action = Action.SLOWER;
        }

        public void port() {
            this.action = Action.PORT;
        }

        public void starboard() {
            this.action = Action.STARBOARD;
        }

        public void placeMine() {
            if (MINES_ENABLED) {
                this.action = Action.MINE;
            }
        }

        public Coord stern() {
            return position.neighbor((orientation + 3) % 6);
        }

        public Coord bow() {
            return position.neighbor(orientation);
        }

        public Coord newStern() {
            return position.neighbor((newOrientation + 3) % 6);
        }

        public Coord newBow() {
            return position.neighbor(newOrientation);
        }

        public bool at(Coord coord) {
            Coord _stern = stern();
            Coord _bow = bow();
            return _stern != null && _stern.equals(coord) || _bow != null && _bow.equals(coord) || position.equals(coord);
        }

        public bool newBowIntersect(Ship other) {
            return newBowCoordinate != null && (newBowCoordinate.equals(other.newBowCoordinate) || newBowCoordinate.equals(other.newPosition)
                    || newBowCoordinate.equals(other.newSternCoordinate));
        }

        public bool newBowIntersect(List<Ship> ships) {
            foreach (Ship other in ships) {
                if (this != other && newBowIntersect(other)) {
                    return true;
                }
            }
            return false;
        }

        public bool newPositionsIntersect(Ship other) {
            bool sternCollision = newSternCoordinate != null && (newSternCoordinate.equals(other.newBowCoordinate)
                    || newSternCoordinate.equals(other.newPosition) || newSternCoordinate.equals(other.newSternCoordinate));
            bool centerCollision = newPosition != null && (newPosition.equals(other.newBowCoordinate) || newPosition.equals(other.newPosition)
                    || newPosition.equals(other.newSternCoordinate));
            return newBowIntersect(other) || sternCollision || centerCollision;
        }

        public bool newPositionsIntersect(List<Ship> ships) {
            foreach (Ship other in ships) {
                if (this != other && newPositionsIntersect(other)) {
                    return true;
                }
            }
            return false;
        }

        public void damage(int health) {
            this.health -= health;
            if (this.health <= 0) {
                this.health = 0;
            }
        }

        public void heal(int health) {
            this.health += health;
            if (this.health > MAX_SHIP_HEALTH) {
                this.health = MAX_SHIP_HEALTH;
            }
        }

        public void fire(int x, int y) {
            if (CANNONS_ENABLED) {
                Coord target = new Coord(x, y);
                this.target = target;
                this.action = Action.FIRE;
            }
        }
    }

    public class Player {
        public int id;
        public List<Ship> ships;
        public List<Ship> shipsAlive;

        public Player(int id) {
            this.id = id;
            this.ships = new List< Ship >();
            this.shipsAlive = new List< Ship >();
        }

        public void setDead() {
            foreach (Ship ship in ships) {
                ship.health = 0;
            }
        }

        public int getScore() {
            int score = 0;
            foreach (Ship ship in ships) {
                score += ship.health;
            }
            return score;
        }

        public List<String> toViewString() {
            List<String> data = new List< String >();

            data.Add("" + (this.id));
            foreach (Ship ship in ships) {
                data.Add(ship.toViewString());
            }

            return data;
        }
    }

    private int seed;
    private List<Cannonball> cannonballs;
    private List<Mine> mines;
    private List<RumBarrel> barrels;
    private List<Player> players;
    private List<Ship> ships = new List< Ship >();
    private List<Damage> damage;
    private List<Ship> shipLosts;
    private List<Coord> cannonBallExplosions;
    private int shipsPerPlayer;
    private int mineCount;
    private int barrelCount;
    private Random random;

    public void initReferee(int playerCount, Properties prop) {
        seed = int.Parse("" + prop.getProperty("seed", "" + (Random.Range(-10000000, 10000000))));
        Random.InitState(seed);

        shipsPerPlayer = clamp(
                Int32.Parse("" + prop.getProperty("shipsPerPlayer", "" + (Random.Range(0, 1 + MAX_SHIPS - MIN_SHIPS) + MIN_SHIPS))), MIN_SHIPS,
                MAX_SHIPS);

        if (MAX_MINES > MIN_MINES) {
            mineCount = clamp(Int32.Parse("" + prop.getProperty("mineCount", "" + (Random.Range(0, MAX_MINES - MIN_MINES) + MIN_MINES))),
                    MIN_MINES, MAX_MINES);
        } else {
            mineCount = MIN_MINES;
        }

        barrelCount = clamp(
                Int32.Parse("" + prop.getProperty("barrelCount", "" + (Random.Range(0, MAX_RUM_BARRELS - MIN_RUM_BARRELS) + MIN_RUM_BARRELS))),
                MIN_RUM_BARRELS, MAX_RUM_BARRELS);

        cannonballs = new List< Cannonball >();
        cannonBallExplosions = new List< Coord >();
        damage = new List< Damage >();
        shipLosts = new List< Ship >();

        // Generate Players
        this.players = new List<Player>(playerCount);
        for (int i = 0; i < playerCount; i++) {
            this.players.Add(new Player(i));
        }
        // Generate Ships
        for (int j = 0; j < shipsPerPlayer; j++) {
            int xMin = 1 + j * MAP_WIDTH / shipsPerPlayer;
            int xMax = (j + 1) * MAP_WIDTH / shipsPerPlayer - 2;

            int y = 1 + Random.Range(0, MAP_HEIGHT / 2 - 2);
            int x = xMin + Random.Range(0, 1 + xMax - xMin);
            int orientation = Random.Range(0, 6);

            Ship ship0 = new Ship(x, y, orientation, 0);
            Ship ship1 = new Ship(x, MAP_HEIGHT - 1 - y, (6 - orientation) % 6, 1);

            this.players[0].ships.Add(ship0);
            this.players[1].ships.Add(ship1);
            this.players[0].shipsAlive.Add(ship0);
            this.players[1].shipsAlive.Add(ship1);
        }

        this.ships.Clear();
        foreach (var ships in players.Select(p => p.ships))
            this.ships.Concat(ships);

        // Generate mines
        mines = new List< Mine >();
        while (mines.Count < mineCount) {
            int x = 1 + Random.Range(0, MAP_WIDTH - 2);
            int y = 1 + Random.Range(0, MAP_HEIGHT / 2);

            Mine m = new Mine(x, y);
            bool valid = true;
            foreach (Ship ship in this.ships) {
                if (ship.at(m.position)) {
                    valid = false;
                    break;
                }
            }
            if (valid) {
                if (y != MAP_HEIGHT - 1 - y) {
                    mines.Add(new Mine(x, MAP_HEIGHT - 1 - y));
                }
                mines.Add(m);
            }
        }
        mineCount = mines.Count;

        // Generate supplies
        barrels = new List< RumBarrel >();
        while (barrels.Count < barrelCount) {
            int x = 1 + Random.Range(0, MAP_WIDTH - 2);
            int y = 1 + Random.Range(0, MAP_HEIGHT / 2);
            int h = MIN_RUM_BARREL_VALUE + Random.Range(0, 1 + MAX_RUM_BARREL_VALUE - MIN_RUM_BARREL_VALUE);

            RumBarrel m = new RumBarrel(x, y, h);
            bool valid = true;
            foreach (Ship ship in this.ships) {
                if (ship.at(m.position)) {
                    valid = false;
                    break;
                }
            }
            foreach (Mine mine in this.mines) {
                if (mine.position.equals(m.position)) {
                    valid = false;
                    break;
                }
            }
            if (valid) {
                if (y != MAP_HEIGHT - 1 - y) {
                    barrels.Add(new RumBarrel(x, MAP_HEIGHT - 1 - y, h));
                }
                barrels.Add(m);
            }
        }

    }

    public Properties getConfiguration() {
        Properties prop = new Properties();
        prop.setProperty("seed", "" + (seed));
        prop.setProperty("shipsPerPlayer", "" + (shipsPerPlayer));
        prop.setProperty("barrelCount", "" + (barrelCount));
        prop.setProperty("mineCount", "" + (mineCount));
        return prop;
    }

    public void prepare(int round) {
        foreach (Player player in players) {
            foreach (Ship ship in player.ships) {
                ship.action = null;
                ship.message = null;
            }
        }
        cannonBallExplosions.Clear();
        damage.Clear();
        shipLosts.Clear();
    }

    public int getExpectedOutputLineCountForPlayer(int playerIdx) {
        return this.players[playerIdx].shipsAlive.Count;
    }

    public void handlePlayerOutput(int frame, int round, int playerIdx, String[] outputs) {
        Player player = this.players[playerIdx];

        int i = 0;
        foreach (String line in outputs) {
            Match matchWait = PLAYER_INPUT_WAIT_PATTERN.Match(line);
            Match matchMove = PLAYER_INPUT_MOVE_PATTERN.Match(line);
            Match matchFaster = PLAYER_INPUT_FASTER_PATTERN.Match(line);
            Match matchSlower = PLAYER_INPUT_SLOWER_PATTERN.Match(line);
            Match matchPort = PLAYER_INPUT_PORT_PATTERN.Match(line);
            Match matchStarboard = PLAYER_INPUT_STARBOARD_PATTERN.Match(line);
            Match matchFire = PLAYER_INPUT_FIRE_PATTERN.Match(line);
            Match matchMine = PLAYER_INPUT_MINE_PATTERN.Match(line);
            Ship ship = player.shipsAlive[i++];

            if (matchMove.Success) {
                int x = Int32.Parse(matchMove.Groups["x"].Value);
                int y = Int32.Parse(matchMove.Groups["y"].Value);
                ship.setMessage(matchMove.Groups["message"].Value);
                ship.moveTo(x, y);
            } else if (matchFaster.Success) {
                ship.setMessage(matchFaster.Groups["message"].Value);
                ship.faster();
            } else if (matchSlower.Success) {
                ship.setMessage(matchSlower.Groups["message"].Value);
                ship.slower();
            } else if (matchPort.Success) {
                ship.setMessage(matchPort.Groups["message"].Value);
                ship.port();
            } else if (matchStarboard.Success) {
                ship.setMessage(matchStarboard.Groups["message"].Value);
                ship.starboard();
            } else if (matchWait.Success) {
                ship.setMessage(matchWait.Groups["message"].Value);
            } else if (matchMine.Success) {
                ship.setMessage(matchMine.Groups["message"].Value);
                ship.placeMine();
            } else if (matchFire.Success) {
                int x = Int32.Parse(matchFire.Groups["x"].Value);
                int y = Int32.Parse(matchFire.Groups["y"].Value);
                ship.setMessage(matchFire.Groups["message"].Value);
                ship.fire(x, y);
            } else {
                Debug.Log("invalid action: " + line);
                break ;
            }
        }
    }

    private void decrementRum() {
        foreach (Ship ship in ships) {
            ship.damage(1);
        }
    }

    private void moveCannonballs() {
        cannonballs.RemoveAll(ball => {
            if (ball.remainingTurns == 0) {
                return true;
            } else if (ball.remainingTurns > 0) {
                ball.remainingTurns--;
            }

            if (ball.remainingTurns == 0) {
                cannonBallExplosions.Add(ball.position);
            }
            return false;
        });
    }

    private void applyActions() {
        foreach (Player player in players) {
            foreach (Ship ship in player.shipsAlive) {
                if (ship.mineCooldown > 0) {
                    ship.mineCooldown--;
                }
                if (ship.cannonCooldown > 0) {
                    ship.cannonCooldown--;
                }

                ship.newOrientation = ship.orientation;

                if (ship.action != null) {
                    switch (ship.action) {
                    case Action.FASTER:
                        if (ship.speed < MAX_SHIP_SPEED) {
                            ship.speed++;
                        }
                        break;
                    case Action.SLOWER:
                        if (ship.speed > 0) {
                            ship.speed--;
                        }
                        break;
                    case Action.PORT:
                        ship.newOrientation = (ship.orientation + 1) % 6;
                        break;
                    case Action.STARBOARD:
                        ship.newOrientation = (ship.orientation + 5) % 6;
                        break;
                    case Action.MINE:
                        if (ship.mineCooldown == 0) {
                            Coord target = ship.stern().neighbor((ship.orientation + 3) % 6);

                            if (target.isInsideMap()) {
                                bool cellIsFreeOfBarrels = !barrels.Any(b => b.position.equals(target));
                                bool cellIsFreeOfShips = !ships.Any(s => s != ship && s.at(target));

                                if (cellIsFreeOfBarrels && cellIsFreeOfShips) {
                                    ship.mineCooldown = COOLDOWN_MINE;
                                    Mine mine = new Mine(target.x, target.y);
                                    mines.Add(mine);
                                }
                            }

                        }
                        break;
                    case Action.FIRE:
                        int distance = ship.bow().distanceTo(ship.target);
                        if (ship.target.isInsideMap() && distance <= FIRE_DISTANCE_MAX && ship.cannonCooldown == 0) {
                            int travelTime = 1 + Mathf.RoundToInt(ship.bow().distanceTo(ship.target) / 3);
                            cannonballs.Add(new Cannonball(ship.target.x, ship.target.y, ship.id, ship.bow().x, ship.bow().y, travelTime));
                            ship.cannonCooldown = COOLDOWN_CANNON;
                        }
                        break;
                    default:
                        break;
                    }
                }
            }
        }
    }

    private bool checkCollisions(Ship ship) {
        Coord bow = ship.bow();
        Coord stern = ship.stern();
        Coord center = ship.position;

        // Collision with the barrels
        barrels.RemoveAll(barrel => {
            if (barrel.position.equals(bow) || barrel.position.equals(stern) || barrel.position.equals(center)) {
                ship.heal(barrel.health);
                return true;
            }
            return false;
        });

        // Collision with the mines
        mines.RemoveAll(mine => {
            List<Damage> mineDamage = mine.explode(ships, false);

            if (!(mineDamage.Count == 0)) {
                damage.AddRange(mineDamage);
                return true;
            }
            return false;
        });

        return ship.health <= 0;
    }

    private void moveShips() {
        // ---
        // Go forward
        // ---
        for (int i = 1; i <= MAX_SHIP_SPEED; i++) {
            foreach (Player player in players) {
                foreach (Ship ship in player.shipsAlive) {
                    ship.newPosition = ship.position;
                    ship.newBowCoordinate = ship.bow();
                    ship.newSternCoordinate = ship.stern();

                    if (i > ship.speed) {
                        continue;
                    }

                    Coord newCoordinate = ship.position.neighbor(ship.orientation);

                    if (newCoordinate.isInsideMap()) {
                        // Set new coordinate.
                        ship.newPosition = newCoordinate;
                        ship.newBowCoordinate = newCoordinate.neighbor(ship.orientation);
                        ship.newSternCoordinate = newCoordinate.neighbor((ship.orientation + 3) % 6);
                    } else {
                        // Stop ship!
                        ship.speed = 0;
                    }
                }
            }

            // Check ship and obstacles collisions
            List<Ship> collisions = new List< Ship >();
            bool collisionDetected = true;
            while (collisionDetected) {
                collisionDetected = false;

                foreach (Ship ship in this.ships) {
                    if (ship.newBowIntersect(ships)) {
                        collisions.Add(ship);
                    }
                }

                foreach (Ship ship in collisions) {
                    // Revert last move
                    ship.newPosition = ship.position;
                    ship.newBowCoordinate = ship.bow();
                    ship.newSternCoordinate = ship.stern();

                    // Stop ships
                    ship.speed = 0;

                    collisionDetected = true;
                }
                collisions.Clear();
            }

            foreach (Player player in players) {
                foreach (Ship ship in player.shipsAlive) {
                    if (ship.health == 0) {
                        continue;
                    }

                    ship.position = ship.newPosition;
                    if (checkCollisions(ship)) {
                        shipLosts.Add(ship);
                    }
                }
            }
        }
    }

    private void rotateShips() {
        // Rotate
        foreach (Player player in players) {
            foreach (Ship ship in player.shipsAlive) {
                ship.newPosition = ship.position;
                ship.newBowCoordinate = ship.newBow();
                ship.newSternCoordinate = ship.newStern();
            }
        }

        // Check collisions
        bool collisionDetected = true;
        List<Ship> collisions = new List< Ship >();
        while (collisionDetected) {
            collisionDetected = false;

            foreach (Ship ship in this.ships) {
                if (ship.newPositionsIntersect(ships)) {
                    collisions.Add(ship);
                }
            }

            foreach (Ship ship in collisions) {
                ship.newOrientation = ship.orientation;
                ship.newBowCoordinate = ship.newBow();
                ship.newSternCoordinate = ship.newStern();
                ship.speed = 0;
                collisionDetected = true;
            }

            collisions.Clear();
        }

        // Apply rotation
        foreach (Player player in players) {
            foreach (Ship ship in player.shipsAlive) {
                if (ship.health == 0) {
                    continue;
                }

                ship.orientation = ship.newOrientation;
                if (checkCollisions(ship)) {
                    shipLosts.Add(ship);
                }
            }
        }
    }

    private bool gameIsOver() {
        foreach (Player player in players) {
            if (player.shipsAlive.Count == 0) {
                return true;
            }
        }
        return barrels.Count == 0 && LEAGUE_LEVEL == 0;
    }

    void explodeShips() {
        cannonBallExplosions.RemoveAll(position => {
            foreach (Ship ship in ships) {
                if (position.equals(ship.bow()) || position.equals(ship.stern())) {
                    damage.Add(new Damage(position, LOW_DAMAGE, true));
                    ship.damage(LOW_DAMAGE);
                    return true;
                } else if (position.equals(ship.position)) {
                    damage.Add(new Damage(position, HIGH_DAMAGE, true));
                    ship.damage(HIGH_DAMAGE);
                    return true;
                }
            }
            return false;
        });
    }

    void explodeMines() {
        cannonBallExplosions.RemoveAll(position => {
            return mines.RemoveAll(mine => {
                if (mine.position.equals(position)) {
                    damage.AddRange(mine.explode(ships, true));
                    return true;
                }
                return false;
            }) > 0;
        });
    }

    void explodeBarrels() {
        cannonBallExplosions.RemoveAll(position => {
            return barrels.RemoveAll(barrel => {
                if (barrel.position.equals(position)) {
                    damage.Add(new Damage(position, 0, true));
                    return true;
                }
                return false;
            }) > 0;
        });
    }

    public void updateGame(int round) {
        moveCannonballs();
        decrementRum();

        applyActions();
        moveShips();
        rotateShips();

        explodeShips();
        explodeMines();
        explodeBarrels();

        foreach (Ship ship in shipLosts) {
            barrels.Add(new RumBarrel(ship.position.x, ship.position.y, REWARD_RUM_BARREL_VALUE));
        }

        foreach (Coord position in cannonBallExplosions) {
            damage.Add(new Damage(position, 0, false));
        }

        ships.RemoveAll(ship => {
            if (ship.health <= 0) {
                players[ship.owner].shipsAlive.Remove(ship);
                return true;
            }
            return false;
        });

        if (gameIsOver()) {
            Debug.Log("Game is Over !");
        }
    }

    public void populateMessages(Properties p) {
        p.put("endReached", "End reached");
    }

    public String[] getInitInputForPlayer(int playerIdx) {
        return new String[0];
    }

    public String[] getInputForPlayer(int round, int playerIdx) {
        List<String> data = new List< String >();

        // Player's ships first
        foreach (Ship ship in players[playerIdx].shipsAlive) {
            data.Add(ship.toPlayerString(playerIdx));
        }

        // Number of ships
        data.Insert(0, "" + (data.Count));

        // Opponent's ships
        foreach (Ship ship in players[(playerIdx + 1) % 2].shipsAlive) {
            data.Add(ship.toPlayerString(playerIdx));
        }

        // Visible mines
        foreach (Mine mine in mines) {
            bool visible = false;
            foreach (Ship ship in players[playerIdx].ships) {
                if (ship.position.distanceTo(mine.position) <= MINE_VISIBILITY_RANGE) {
                    visible = true;
                    break;
                }
            }
            if (visible) {
                data.Add(mine.toPlayerString(playerIdx));
            }
        }

        foreach (Cannonball ball in cannonballs) {
            data.Add(ball.toPlayerString(playerIdx));
        }

        foreach (RumBarrel barrel in barrels) {
            data.Add(barrel.toPlayerString(playerIdx));
        }

        data.Insert(1, "" + (data.Count - 1));

        return data.ToArray();
    }

    public String[] getInitDataForView() {
        List<String> data = new List< String >();

        data.Add(join(MAP_WIDTH, MAP_HEIGHT, players[0].ships.Count, MINE_VISIBILITY_RANGE));

        data.Insert(0, "" + (data.Count + 1));

        return data.ToArray();
    }

    public void getFrameDataForView(
        List< Player > players,
            List< Cannonball > cannonBalls,
            List< Mine > mines,
            List< RumBarrel > rumBarrels,
            List< Damage > damages) {

        players.AddRange(this.players);
        cannonBalls.AddRange(this.cannonballs);
        mines.AddRange(this.mines);
        rumBarrels.AddRange(this.barrels);
        damages.AddRange(this.damage);

        /*List<String> data = new List< String >();

        foreach (Player player in players) {
            data.AddRange(player.toViewString());
        }
        data.Add("" + (cannonballs.Count));
        foreach (Cannonball ball in cannonballs) {
            data.Add(ball.toViewString());
        }
        data.Add("" + (mines.Count));
        foreach (Mine mine in mines) {
            data.Add(mine.toViewString());
        }
        data.Add("" + (barrels.Count));
        foreach (RumBarrel barrel in barrels) {
            data.Add(barrel.toViewString());
        }
        data.Add("" + (damage.Count));
        foreach (Damage d in damage) {
            data.Add(d.toViewString());
        }

        return data.ToArray();*/
    }

    public String getGameName() {
        return "CodersOfTheCaribbean";
    }

    public String getHeadlineAtGameStartForConsole() {
        return null;
    }

    public int getMinimumPlayerCount() {
        return 2;
    }

    public bool showTooltips() {
        return true;
    }

    public String[] getPlayerActions(int playerIdx, int round) {
        return new String[0];
    }

    public bool isPlayerDead(int playerIdx) {
        return false;
    }

    public String getDeathReason(int playerIdx) {
        return "$" + playerIdx + ": Eliminated!";
    }

    public int getScore(int playerIdx) {
        return players[playerIdx].getScore();
    }

    public String[] getGameSummary(int round) {
        return new String[0];
    }

    public void setPlayerTimeout(int frame, int round, int playerIdx) {
        players[playerIdx].setDead();
    }

    public int getMaxRoundCount(int playerCount) {
        return 200;
    }

    public int getMillisTimeForRound() {
        return 50;
    }
}