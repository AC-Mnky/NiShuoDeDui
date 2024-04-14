<!-- code transition between gamescene -->
<!-- draw reddoor, bluedoor -->
<!-- draw shop, inventory -->
<!-- draw font, code font -->
<!-- code inventory -->
<!-- code shop -->
---0.1.0---
<!-- code stage -->
<!-- (not urgent)better collision -->
<!-- spell description -->
<!-- fix stage: towers into inventory -->
<!-- mana -->
<!-- spells have different mana -->
<!-- (not urgent) font ,. -->
<!-- shop shows price -->
<!-- manaMul -->
<!-- more enemies -->
<!-- code dummies -->
<!-- rotation and arrow texture -->
<!-- pause and double speed -->
<!-- begin battle button -->
<!-- bluereddoor fix -->
<!-- bluereddoor at center -->
<!-- improve title : introduction to fullscreen, map dragging, loop space, options, quit [this is a demo version that does not represent the final quality of the game] -->
<!-- more more enemies -->
<!-- more spells -->
<!-- scaling spell -->
---0.1.3---
UI: fix manamul description bug
UI: entity component ui
UI: spellcast 
UI: square and circle spellslots
UI: tower damage display
Preview: less entities
Core gameplay: (big changes) players should not hold so many spells at once (or players should not edit spells freely(spells come in packs))
Save
Gameplay: better shop design (more expensive, different spells for different stages)
Gameplay: stage 3 enemies
Core gameplay: boss fight
UI: multicast manamul (*2)
Core gameplay: (not urgent)draw perk, code perk
Gameplay: (not urgent)better map generation
Gameplay: (not urgent)better better collision
Gameplay: much more spells
Tutorial: make it better
(?)Core gameplay: physics / magic projectiles?
QOL coding: entity-component matrix
UI: 3*3*3 inventory
(?)Gameplay: Health regen after boss fight?




To add a spell:
Thing.cs/Name
Spell.cs/description
(?)Spell.cs/dependentonly
Spellcast.cs/TickUpdate
Game1.cs/RandomSpellName
Game1.cs/Price, Cost, manamul
(draw icon)
(mgcb editor)
(Game1.cs/_TextureIcon)

To add a projectile spell:
Thing.cs/projectile Name
Entity.cs/a lot of shit
Entity.cs/Damage
Projectile.cs/TickUpdate
Spell.cs/Projectile description
Game1.cs/RandomProjectileName
Game1.cs/Price, Cost, manamul
(draw projectile)
(mgcb editor)
Game1.cs/Texture

To add an enemy:
Thing.cs/name
Entity.cs/a lot of shit
Game1.cs/GenerateCardDeck
Enemy/TickUpdate
(draw texture)
(mgcb editor)
Game1.cs/Texture