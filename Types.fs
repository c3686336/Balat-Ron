module Types

type Tile =
  | Tile of int

  override this.ToString() =
    let (Tile v) = this
    [|"Wat";"🀐";"🀑";"🀒";"🀓";"🀔";"🀕";"🀖";"🀗";"🀘"|][v]

  member this.Value() =
    let (Tile v) = this
    v

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

type Hand =
  ArrayHand * Kantsu list

type ParsedNormalHand =
  | ParsedHand of Kantsu list * Shuntsu list * Kotsu list * Toitsu

  override this.ToString (): string =
    let (ParsedHand (kan, shun, ko, toi)) = this
    $"Kantsu: {kan}, Shuntsu: {shun}, Kotsu: {ko}, Toitsu: {toi}"

type ParsedChitoitsu =
  | ParsedChitoitsu of Tile list

  override this.ToString (): string = 
    let (ParsedChitoitsu (a::b::c::d::e::f::g)) = this
    $"Chitoitsu: {a}{a}{b}{b}{c}{c}{d}{d}{e}{e}{f}{f}{g}{g}"

type ParsedHand =
  | NormalHand of ParsedNormalHand
  | Chitoitsu of ParsedChitoitsu
