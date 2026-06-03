internal static class WeaponIndex
{
    internal static string GetWeaponName(int index)
    {
        return index switch
        {
            // Assault Rifles
            0 => "AK47",
            2 => "M4A1",
            6 => "AK47",

            12 => "SCAR",
            178 => "SCAR-I",
            179 => "SCAR-II",
            180 => "SCAR-III",

            14 => "GROZA",
            70 => "GROZA-X",

            24 => "FAMAS",
            67 => "FAMAS-I",
            130 => "FAMAS-II",
            131 => "FAMAS-III",

            28 => "XM8",
            33 => "AN94",

            46 => "AUG",
            193 => "AUG-I",
            194 => "AUG-II",
            195 => "AUG-III",

            47 => "PARAFAL",
            57 => "KINGFISHER",

            73 => "G36-ASSAULT",
            74 => "G36-RANGE",

            80 => "M4A1-I",
            81 => "M4A1-II",
            82 => "M4A1-III",

            // SMG
            7 => "UMP",

            8 => "MP5",
            60 => "MP5-I",
            120 => "MP5-II",
            121 => "MP5-III",

            15 => "MP40",
            32 => "P90",

            21022 => "THOMPSON",
            21020 => "THOMPSON-X",

            49 => "VECTOR",
            58 => "MINI UZI",

            88 => "MAC10",
            228 => "MAC10-I",
            229 => "MAC10-II",
            230 => "MAC10-III",

            150 => "BIZON",

            // Shotguns
            5 => "M1014",
            184 => "M1014-I",
            185 => "M1014-II",
            186 => "M1014-III",

            20 => "M1873",
            29 => "SPAS12",

            41 => "M1887",
            119 => "M1887-X",

            50 => "MAG7",
            86 => "CHARGE BUSTER",
            181 => "TROGON",
            21002 => "M590",

            // Snipers / Marksman
            4 => "AWM",
            65 => "AWM-Y",

            11 => "M14",
            63 => "M14-I",
            126 => "M14-II",
            127 => "M14-III",

            13 => "VSS",
            62 => "VSS-I",
            124 => "VSS-II",
            125 => "VSS-III",

            18 => "SKS",

            21 => "KAR98K",
            64 => "KAR98K-I",
            128 => "KAR98K-II",
            129 => "KAR98K-III",

            26 => "SVD",
            72 => "SVD-Y",

            45 => "M82B",
            48 => "WOODPECKER",

            75 => "M24",
            805 => "M24-I",
            806 => "M24-II",
            807 => "M24-III",

            78 => "HEAL SNIPER",

            89 => "AC80",
            197 => "VSK94",

            21006 => "WINCHESTER",
            21007 => "WINCHESTER-I",
            21008 => "WINCHESTER-II",
            21009 => "WINCHESTER-III",

            // LMG
            19 => "M249",
            71 => "M249-X",

            30 => "M60",
            61 => "M60-I",
            122 => "M60-II",
            123 => "M60-III",

            54 => "KORD",

            // Pistols
            3 => "USP",
            9 => "DESERT EAGLE",
            10 => "G18",
            25 => "M500",

            55 => "M1917",
            56 => "USP2",

            93 => "HEAL PISTOL",
            21001 => "HEAL PISTOL Y",

            // Melee
            1 => "FIST",
            16 => "PAN",
            17 => "PARANG",
            27 => "BAT",
            34 => "KATANA",
            51 => "SCYTHE",
            149 => "KNIFE",

            // Special Weapons
            38 => "HOOK GUN",
            35 => "CG15",
            39 => "PLASMA",
            99 => "SHIELD GUN",
            100 => "FLAME THROWER",
            37 => "GATLING",
            36 => "RGS50",
            196 => "FGL24",


            _ => $"{index}"
        };
    }
}