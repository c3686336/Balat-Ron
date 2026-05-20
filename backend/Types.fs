module Types

open System

type Rarity =
  | Common
  | Uncommon
  | Rare
  | Legendary
  | Mythical

  override this.ToString() =
    match this with
    | Common -> "Common"
    | Uncommon -> "Uncommon"
    | Rare -> "Rare"
    | Legendary -> "Legendary"
    | Mythical -> "Mythical"

type Tile =
  | Tile of int

  override this.ToString() =
    let (Tile v) = this
    [|"Wat";"🀐";"🀑";"🀒";"🀓";"🀔";"🀕";"🀖";"🀗";"🀘"|][v]

  member this.Value() =
    let (Tile v) = this
    v

  member this.DoraTile() =
    let (Tile v) = this
    Tile <| v % 9 + 1

  member this.IsValid () =
    1 <= this.Value () && this.Value () <= 9

  member this.IsTerminal () =
    this.Value () = 1 || this.Value () = 9

  member this.NextTile (wrapAround: bool) =
    if wrapAround then Some (this.DoraTile ()) else if this.Value () = 9 then None else Some (Tile (1 + this.Value ()))

type Kantsu =
  | Kantsu of Tile

  override this.ToString (): string =
    let (Kantsu t) = this
    $"{t}🀫🀫{t}"

type Shuntsu =
  | Shuntsu of Tile * Tile * Tile

  override this.ToString (): string =
    let (Shuntsu (a, b, c)) = this
    $"{a}{b}{c}"

type Kotsu =
  | Kotsu of Tile

  override this.ToString (): string =
    let (Kotsu t) = this
    $"{t}{t}{t}"

type Toitsu =
  | Toitsu of Tile

  override this.ToString (): string =
    let (Toitsu t) = this
    $"{t}{t}"

type ListHand =
  | ListHand of (Tile * int) list // Tile's name and count

type ArrayHand = int array

let arrayHandToString (arrayHand: ArrayHand): string =
  arrayHand
        |> Array.mapi (fun i x -> String.replicate x $"{Tile i}") |> String.concat ""

type CliInput =
  | Tsumo
  | Kan of Tile
  | Discard of Tile
  | EmptyPile

  static member TryParse (input: string) =
    let truncated = input.Trim ()

    match truncated with
      | "t" -> Some (Tsumo)
      | str ->
        match Int32.TryParse (str) with
          | (true, value) when 1 <= value && value <= 9 -> Some (Discard (Tile value))
          | (true, _) -> None
          | (false, _) ->
            if str.StartsWith("k") then
              match Int32.TryParse (str.Substring(1)) with
                | (true, value) when 1 <= value && value <= 9 -> Some (Kan (Tile value))
                | _ -> None
            else
              None

type Hand =
  | Hand of ArrayHand * Tile * Kantsu list

  override this.ToString (): string =
    let (Hand (arrayHand, tsumo, kantsu)) = this

    let playableHandString =
      arrayHand
        |> arrayHandToString

    let kantsuString =
      kantsu
        |> List.map (fun x -> $"{x}") |> String.concat " "

    $"{playableHandString} {tsumo}  {kantsuString}"

  member this.PlayableHand () =
    let (Hand (arrayHand, _, _)) = this
    arrayHand

  member this.Kantsu () =
    let (Hand (_, _, kantsu)) = this
    kantsu

  member this.Tsumo () =
    let (Hand (_, tsumo, _)) = this
    tsumo

  member this.IsKanValid (t: Tile) =
    this.PlayableHand()[t.Value()] >= 4 || this.Tsumo () = t && this.PlayableHand()[t.Value()] >= 3

  member this.IsDiscardValid (t: Tile) =
    this.PlayableHand()[t.Value()] >= 1 || this.Tsumo () = t

  member this.Discard (t: Tile) (newTsumo: Tile)=
    let (Hand (arrayHand, tsumo, kantsu)) = this
    if t <> this.Tsumo () then
      let discardedArrayHand = Array.updateAt (t.Value()) (arrayHand[t.Value()] - 1) arrayHand
      let arrayHandWithTsumo = Array.updateAt (tsumo.Value()) (discardedArrayHand[tsumo.Value()] + 1) discardedArrayHand

      Hand (arrayHandWithTsumo, newTsumo, kantsu)
    else
      Hand (arrayHand, newTsumo, kantsu)

  member this.Kan (t: Tile) (newTsumo: Tile) =
    let (Hand (arrayHand, tsumo, kantsu)) = this
    if t <> this.Tsumo () then
      let kannedArrayHand = Array.updateAt (t.Value()) (arrayHand[t.Value()] - 4) arrayHand
      let arrayHandWithTsumo = Array.updateAt (tsumo.Value()) (kannedArrayHand[tsumo.Value()] + 1) kannedArrayHand

      Hand (arrayHandWithTsumo, newTsumo, Kantsu t :: kantsu)
    else
      let kannedArrayHand = Array.updateAt (t.Value()) (arrayHand[t.Value()] - 3) arrayHand

      Hand (kannedArrayHand, newTsumo, Kantsu t :: kantsu)

type ParsedNormalHand =
  | ParsedHand of Kantsu list * Shuntsu list * Kotsu list * Toitsu

  override this.ToString (): string =
    let (ParsedHand (kan, shun, ko, toi)) = this
    $"Kantsu: {kan}, Shuntsu: {shun}, Kotsu: {ko}, Toitsu: {toi}"

type ParsedChitoitsu =
  | ParsedChitoitsu of Tile list

  override this.ToString (): string =
    let (ParsedChitoitsu tiles) = this
    let body = tiles |> List.map (fun t -> $"{t}{t}") |> String.concat ""
    $"Chitoitsu: {body}"

type Machi =
  | Tanki of Tile
  | Shanponmachi of Tile * Tile
  | Penchanmachi of Tile * Tile
  | Kanchanmachi of Tile * Tile
  | Ryoumenmachi of Tile * Tile

type ParsedHand =
  | NormalHand of ParsedNormalHand
  | Chitoitsu of ParsedChitoitsu

type DoraIndicator =
  Tile list

type Pile =
  Tile array

type GamePhase =
  | GameStarting
  | InGame
  | ScorePresentation
  | Shop
  | GameOver

type ShuffleScope =
  | Everything
  | DiscardToDrawPile
  | UnrevaledTilesOnly

type GameState =
  {rng: Random; hand: Hand; pile: Pile; discardPile: Pile; doraPile: Pile; dora: Pile; rinshang: Pile; round: int; honbaLeft: int; isRinshanKaihouApplicable: bool; isTenhouApplicable: bool; items: Item list; currentScore: bigint; goalScore: bigint; gold: int; baseScore: int * int; phase: GamePhase; shopItems: Item list }

  override this.ToString (): string =
    let hand = sprintf $"{this.hand}"
    let dora = this.dora |> Array.map (fun x -> x.ToString()) |> String.concat ""
    let discardPile = this.discardPile |> Array.map (fun x -> x.ToString()) |> String.concat ""

    $"{hand}\n{dora}\nDiscard: {discardPile}"

and ItemEvent =
  | OnYakuCalc of ParsedHand * Machi * Tile * Hand // Hypothetical substitutions
  | OnScoreCalc of ParsedHand * Machi * Tile * Hand // Actual score calculation, Deprecated. Just use OnYakuCalc
  | OnDiscard of Tile
  | OnKan of Tile
  | OnRoundEnd
  | OnTsumo
  | WhenObtained
  | WhenSold
  | WhenPileEmpty // Deprecated; manually detect empty pile
  | PileReset // Deprecated for now
  | Honba
  | Parsing

and ItemEffect =
  | ExtraScore of int * int
  | AddHonba of int
  | AddGold of int
  | UpdateItemState of ItemState
  | SelfDestruct
  | PrintName // Deprecated
  | PrintStr of string // Deprecated
  | DiscloseNMoreDora of int
  | DiscloseUraDora
  | AllowWrapAroundShuntsu
  | ShufflePile of ShuffleScope
  | SuppressHonba

and ItemEffects = Map<ItemEvent, (Item * ItemEffect list) list>

and ItemState =
  | Integer of int
  | Nothing

and Item =
  { id: Guid
    name: string
    description: string
    rarity: Rarity
    effect: GameState -> Item -> ItemEvent -> ItemEffect list
    state: ItemState
    cost: int }

  override this.ToString(): string =
    $"[{this.rarity}] {this.name} ({this.cost}G): {this.description}"

type ScoreReason =
  | Dora
  | BaseFu
  | ItemEffect of Item

type GameEvents =
  | GameStarted
  | TileDiscarded of Tile
  | TileDrawn of Tile
  | RinshangDrawn of Tile
  | PileEmpty // Only when the Pile is really empty
  | Kan of Tile
  | Scored of int * int * bigint
  | EarnedExtraScore of int * int * ScoreReason
  | EarnedExtraHonba of int
  | ScoringStarted
  | EffectTriggered of ItemEvent * Item * ItemEffect list
  | ShopEntered
  | PresentedItem of Item list
  | Bought of Item
  | Sold of Item
  | EarnedGold of int
  | ShuffledPile of ShuffleScope
  | DrawnHand of Hand
  | RevealedDora of Tile list
  | RevealedUraDora of Tile list
  | TransitionedPhaseTo of GamePhase
  | NotEnoughGold
  | InventoryFull
  | ShopItemNotFound
  | ItemTriggered of Item
  | UpdatedItemState of Item * ItemState
  | ItemDestroyed of Item
  | NextRound of int
  | PeekDrawPile of int * Tile // Index, tile
  | PeekRinshang of int * Tile
  | GameOverEvent

type PlayerInput =
  // GameStarting
  | Start
  // InGame
  | Discard of Tile
  | DeclareKan of Tile
  | DeclareTsumo
  | ConfirmPileEmpty
  // Shop
  | Buy of Item
  | Sell of Item
  | ExitShop
  // Score Presentation
  | ConfirmScore
