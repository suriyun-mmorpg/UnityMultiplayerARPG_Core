# Unity Multiplayer Action RPG
This is multiplayer Action RPG which controlling as third person Action RPG

## Character Stats
** Stats **

	* LV (Character Level)
	* HP (Health point)
	* MP (Mana point)
	* ATK (Attack)
	* ATKR (Attack Rating)
	* DEF (Defend)
	* CRIHIT (Critical Hit Rate)
	* CRIDMG (Critical Damage Rate)

** Elemental Atttack **

	* Cold Attack
	* Fire Attack
	* Lightning Attack
	* Poison Attack
	* Magic Attack

** Elemental Resistance **

	* Cold Resistance
	* Fire Resistance
	* Lightning Resistance
	* Poison Resistance
	* Magic Resistance

** Future Plan **

	* ASPD (Attack Speed)
	* MSPD (Move Speed)

** Damage Calculation **

	* Is Crit = Random.value <= CRIHIT
	* Hit Chance = 100 x A.ATKR / (A.ATKR + B.DEF) x 2 x A.LV / (A.LVL + B.LVL)
	* Element Damage = A.EDMG * B.ERES / 100
	* Is Miss = hitChance < 0 || Random.value > hitChance
	* Total Damage = ATK * (isCrit ? CRITDMG : 1)

** Attributes **

	* STR +Weapon's Effectiveness% ATK
	* DEX +0.25 DEF, +5 ATKR
	* VIT +2 HP
	* INT +2 MP
