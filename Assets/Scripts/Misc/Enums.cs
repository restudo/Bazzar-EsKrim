public enum InventoryLocation
{
    Base,
    Flavor,
    Topping,
    count
}

public enum IngredientType
{
    Base,
    Flavor,
    Topping,
    none,
    count
}

public enum IngredientName
{
    none = 0,
    Cone1 = 1001,
    Cone2 = 1002,
    Cone3 = 1003,
    Cone4 = 1004,
    Cone5 = 1005,
    Cone6 = 1006,

    ChocolateFlavor = 1007,
    VanillaFlavor = 1008,
    StrawberryFlavor = 1009,
    MatchaFlavor = 1010,
    BerryFlavor = 1011,
    CandyFlavor = 1012,
    BubblegumFlavor = 1013,
    MoccaFlavor = 1014,
    MangoFlavor = 1015,
    TaroFlavor = 1016,

    AlmondTopping = 1017,
    MessesTopping = 1018,
    CherryTopping = 1019,
    BiscuitTopping = 1020,
    ChocochipTopping = 1021,
    MarshmallowTopping = 1022,
    AstorTopping = 1023,
    NutsTopping = 1024,
    OrangeTopping = 1025,
    StrawberryTopping = 1026
}

public enum Scenes
{
    Level,
    Menu
}

public enum GameStates
{
    MainMenu,
    Collection,
    CollectionPanel,
    LevelSelection,
    MainGame,
    MiniGame
}

public enum ConeTypes
{
    NotACone,
    High,
    Medium,
    Low,
    VeryLow
}

public enum AspectRatio
{
    none,
    Aspect22_9,
    Aspect21_9,
    Aspect20_9,
    Aspect19_9,
    Aspect18_9,
    Aspect16_9,
    Aspect16_10,
    Aspect5_3,
    count
}

public enum Characters
{
    Pariz,
    Coboz
}

public enum Customers
{
    Nyonya,
    Remaja,
    Bapak,
}