﻿using ACE.Database;
using ACE.Managers;

namespace ACE.Network
{
    public static class CharacterHandler
    {
        [Fragment(FragmentOpcode.CharacterDelete)]
        public static void CharacterDelete(ClientPacketFragment fragment, Session session)
        {
            string account     = fragment.Payload.ReadString16L();
            uint characterSlot = fragment.Payload.ReadUInt32();

            if (account != session.Account)
            {
                session.SendCharacterError(CharacterError.Delete);
                return;
            }

            uint guid;
            if (!session.CharacterSlots.TryGetValue((byte)characterSlot, out guid))
            {
                session.SendCharacterError(CharacterError.Delete);
                return;
            }

            // TODO: check if character is already pending removal

            var characterDelete         = new ServerPacket(0x0B, PacketHeaderFlags.EncryptedChecksum);
            var characterDeleteFragment = new ServerPacketFragment(9, FragmentOpcode.CharacterDelete);
            characterDelete.Fragments.Add(characterDeleteFragment);

            NetworkManager.SendLoginPacket(characterDelete, session);

            DatabaseManager.Character.ExecutePreparedStatement(CharacterPreparedStatement.CharacterDelete, WorldManager.GetUnixTime() + 3600ul, guid);
            DatabaseManager.Character.SelectPreparedStatementAsync(AuthenticationHandler.CharacterListSelectCallback, session, CharacterPreparedStatement.CharacterListSelect, session.Id);
        }

        [Fragment(FragmentOpcode.CharacterCreate)]
        public static void CharacterCreate(ClientPacketFragment fragment, Session session)
        {
            //fragment.OutputDataToConsole(false, true, false, 4);

            /*
                                                              [       ==== FACE ====     ==== FACE ====      ][   === CLOTHES ====    === CLOTHES ====    === CLOTHES ====   ]                                                                                                        [==                ATTRIBUTES                ==][           THIS BLOCK ALWAYS SEEMS THE SAME                           ][ ====== SKILLS ====             ====== SKILLS ====          ====== SKILLS ====          ====== SKILLS ====          ====== SKILLS ====          ====== SKILLS ====          ====== SKILLS ====          ====== SKILLS ====          ====== SKILLS ====          ====== SKILLS ====          ====== SKILLS ====          ====== SKILLS ====          ====== SKILLS ====          ====== SKILLS ====    ]
            [     ACCOUNT NAME       ][*1    ][ Race ][ Sex  ][Eyes  ][Nose  ][Mouth ][HairC1][Eye Co][Hair  ][Headge][HeadC1][Shirt ][ShirC1][Trouse][TrouC1][Footwe][FootC1]      [Skin Color    ][Hair Color    ][Headgear Color][Shirt Color   ][Trousers Color][Footwear Color]??[Streng][Endura][Coordi][Quickn][Focus ][ Self ][      ][      ][      ][      ][      ][      ][      ][      ][      ][MeleeD][MissiD][      ][      ][      ][      ][      ][      ][Arcane][MagicD][Mana C][      ][Item T][AssesP][Decept][Healin][ Jump ][Lockpi][ Run  ][      ][      ][AssesC][WeapoT][ArmorT][MagiIT][CreatE][Item E][Life M][War Ma][Leader][Loyalt][Fletch][Alchem][Cookin][Salvag][Two Ha][      ][Void M][HeavyW][LightW][Finess][MissiW][Shield][Dual W][Reckle][SneakA][DirtyF][      ][Summon]      [Name][Tn]                      [??]
            0B006163636F756E746E616D650000000100000004000000010000000F000000060000001700000000000000000000001400000000000000020000000100000002000000020000000200000003000000020000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000200000064000000280000006400000032000000140000001400000000000000010000003700000000000000000000000000000000000000000000000000000003000000010000000000000000000000000000000000000000000000000000000300000002000000010000000000000001000000010000000100000002000000020000000100000002000000000000000000000001000000010000000100000001000000010000000200000001000000010000000100000002000000010000000100000001000000020000000100000000000000010000000300000001000000010000000100000001000000030000000100000001000000010000000000000001000000010041000300000000000000000000009701000000000000
            0B006163636F756E746E616D650000000100000001000000010000000B000000020000000300000001000000010000000D000000000000000300000002000000030000000200000003000000010000000300000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000006000000640000003C00000064000000320000000A0000000A00000000000000010000003700000000000000000000000000000000000000000000000000000003000000010000000000000000000000000000000000000000000000000000000200000002000000010000000000000001000000010000000100000002000000020000000100000002000000000000000000000001000000010000000100000001000000010000000100000001000000010000000100000002000000010000000100000001000000020000000100000000000000010000000300000001000000010000000200000003000000010000000100000001000000030000000000000001000000010041000000000000000000000000007601000000000000
            Above is all hue selectors at the top
            Below is all hue selectors somewhere in the middle
            0B006163636F756E746E616D6500000001000000010000000100000002000000000000000B000000000000000000000003000000FFFFFFFF000000000200000005000000000000000200000000000000060000009D6C4E36279BC33FAA0A5585AA42E53FC99A644DB226D93F98F84BFC25FEB23FA0F24FF9A7FCD33FB407DA03ED81E63F050000001E0000001E00000064000000640000003C0000000A00000000000000010000003700000000000000000000000000000000000000000000000000000002000000010000000000000000000000000000000000000000000000000000000200000002000000010000000000000001000000010000000100000002000000020000000200000002000000000000000000000001000000010000000100000001000000010000000200000001000000010000000100000002000000010000000100000001000000020000000100000000000000010000000100000001000000030000000200000001000000030000000100000001000000030000000000000001000000010041000000000000000000000000006201000000000000
            0B006163636F756E746E616D650000000100000001000000010000000F0000000000000019000000010000000000000024000000FFFFFFFF000000000200000008000000010000000800000001000000030000008A38459C224EE13F9E384F9C27CEE33FA213D189E844E43F88F843FC21FEB03FBE205F902FC8A73FE4D471EA3875CC3F0200000064000000280000006400000032000000140000001400000000000000010000003700000000000000000000000000000000000000000000000000000003000000010000000000000000000000000000000000000000000000000000000300000002000000010000000000000001000000010000000100000002000000020000000100000002000000000000000000000001000000010000000100000001000000010000000200000001000000010000000100000002000000010000000100000001000000020000000100000000000000010000000300000001000000010000000100000001000000030000000100000001000000010000000000000001000000010041000000000000000000000000009E01000000000000
            0B006163636F756E746E616D65000000010000000100000001000000020000000000000017000000020000000200000027000000FFFFFFFF00000000020000000700000000000000050000000000000005000000EF4277A1BBD0DD3F835841AC2056D03F895DC42E6217E13FB4C859E42C72C63FD4E069F03478AA3F878043C021E0903F06000000640000003C00000064000000320000000A0000000A00000000000000010000003700000000000000000000000000000000000000000000000000000003000000010000000000000000000000000000000000000000000000000000000200000002000000010000000000000001000000010000000100000002000000020000000100000002000000000000000000000001000000010000000100000001000000010000000100000001000000010000000100000002000000010000000100000001000000020000000100000000000000010000000300000001000000010000000200000003000000010000000100000001000000030000000000000001000000010041000000000000000000000000009701000000000000
            0B006163636F756E746E616D650000000100000004000000010000000E000000050000000F000000030000000300000012000000FFFFFFFF00000000000000000A000000010000000D0000000100000003000000D195E84A7425EA3F96DC4A6E25B7D23FFB607DB03E58DF3FD60BEB85F5C2EA3FCC11E6087384E93FC9A3E451F228E93F050000001E0000001E00000064000000640000003C0000000A00000000000000010000003700000000000000000000000000000000000000000000000000000002000000010000000000000000000000000000000000000000000000000000000200000002000000010000000000000001000000010000000100000002000000020000000200000002000000000000000000000001000000010000000100000001000000010000000200000001000000010000000100000002000000010000000100000001000000020000000100000000000000010000000100000001000000030000000200000001000000030000000100000001000000030000000000000001000000010041000300000000000000000000008F01000000000000
            0B006163636F756E746E616D65000000010000000200000001000000110000001200000007000000020000000000000021000000FFFFFFFF00000000030000000E0000000100000006000000000000000A000000D19FE84FF427EA3F834A41A5A052D03FF15078283C14BE3FE638739C39CEBC3FC76863B431DAD83FF4087A043D82DE3F0300000028000000320000000A0000001E00000064000000640000000000000001000000370000000000000000000000000000000000000000000000000000000100000001000000000000000000000000000000000000000000000000000000030000000200000002000000000000000100000001000000010000000100000002000000010000000200000000000000000000000100000001000000010000000100000002000000010000000300000002000000010000000200000001000000010000000100000002000000010000000000000001000000010000000100000001000000010000000100000001000000010000000100000001000000000000000100000020004100020000000000000000000000A001000000000000
            Below is all hue selectors at the bottom
            0B006163636F756E746E616D6500000001000000030000000200000007000000050000000900000000000000000000001D0000000000000004000000020000000400000002000000040000000100000004000000000000000000F03F000000000000F03F000000000000F03F000000000000F03F000000000000F03F000000000000F03F0400000032000000280000000A0000001E000000640000006400000000000000010000003700000000000000000000000000000000000000000000000000000001000000010000000000000000000000000000000000000000000000000000000200000002000000030000000000000001000000010000000100000001000000020000000100000002000000000000000000000001000000010000000100000001000000010000000100000002000000030000000100000002000000010000000100000001000000020000000100000000000000010000000100000001000000010000000100000001000000010000000100000001000000010000000000000001000000010041000100000000000000000000008A01000000000000
            0B006163636F756E746E616D6500000001000000040000000100000004000000130000000B00000001000000030000000700000001000000020000000300000007000000010000000D0000000000000007000000000000000000F03F000000000000F03F000000000000F03F000000000000F03F000000000000F03F000000000000F03F01000000280000001E0000006400000064000000320000000A00000000000000010000003700000000000000000000000000000000000000000000000000000003000000010000000000000000000000000000000000000000000000000000000300000002000000010000000000000001000000010000000100000001000000020000000100000002000000000000000000000001000000010000000100000001000000010000000200000001000000010000000100000002000000010000000100000001000000020000000100000000000000010000000100000001000000030000000300000002000000010000000100000001000000010000000000000001000000010041000300000000000000000000008201000000000000


            [*1    ]
            This value is always 00000001

            [ Race ]
            Aluvian			01
            Gharu'ndim		02
            Sho				03
            Viamontian		04
            Umoraen			05
            Penumboraen		0A
            Gear Knight		06
            Undead			0B
            Empyrean		09
            Aun Tumerok		07
            Lugian			08
            Olthoi Soldier	0C
            Olthoi Spitter	0D

            [ Sex  ]
            Male	01
            Female	02

            [HairC1]
            This is the hair main color circle selected.
            Valid valies are between 01-03

            [Headge]
            00000001 = Some sort of headgear
            00000002 = Some sort of headgear
            000000FF = no headgear

            [HeadC1]
            This is the headgear main color circle selected.
            If no headgear is selected, the value is FFFFFF00

            [ShirC1]
            This is the shirt main color circle selected.

            [TrouC1]
            This is the trousers main color circle selected.

            [FootC1]
            This is the footwear main color circle selected.

            [Skills]
            UnusableUntrained	00
            UseableUntrained	01
            Trained				02
            Specialized			03

            [Tn]
            This field might only be 1 byte, instead of a WORD....
            It's also possible that there MAY be a padding byte in front of it sometimes (not 100% sure) to align the data fields...
            The padding byte also might just be at the end of the strings to keep them even as well.. not 100% sure.. since Tn comes right after Name
            Holtburg	00
            Shoushi		01
            Yaraq		02
            Sanamar		03

            [??]
            I believe this to be a checksum of sorts
            */




            /*
            SERVER -> CLIENT RESPONSE

            43 F6 00 00 04 00 00 00     Invalid name response (name is not allowed.. vulgur, etc...)
            43 F6 00 00 03 00 00 00     Name in use response

            Accepted
            43 F6 00 00 01 00 00 00 9E 16 14 50 0F 00 4D 61 67 2D 6E 75 73 61 73 64 66 61 73 64 66 00 00 00 00 00 00 00
            */

            // This works, but I don't know if these values are correct. I just copied and pasted these from PacketManager.cs
            var serverName = new ServerPacket(0x0B, PacketHeaderFlags.EncryptedChecksum);
            var serverNameFragment = new ServerPacketFragment(9, FragmentOpcode.CharacterCreateResponse);

            //serverNameFragment.Payload.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 }); // Cannot create a character at this time
            //serverNameFragment.Payload.Write(new byte[] { 0x02, 0x00, 0x00, 0x00 }); // Cannot create a character at this time
            //serverNameFragment.Payload.Write(new byte[] { 0x03, 0x00, 0x00, 0x00 }); // Name already in use
            //serverNameFragment.Payload.Write(new byte[] { 0x04, 0x00, 0x00, 0x00 }); // Name is not permitted
            //serverNameFragment.Payload.Write(new byte[] { 0x05, 0x00, 0x00, 0x00 }); // Cannot create a character at this time

            // This is a valid response
            // Not sure what this is 0x9E, 0x16, 0x14, 0x50
            // This is the newly created characters name 0x0F, 0x00, 0x4D, 0x61, 0x67, 0x2D, 0x6E, 0x75, 0x73, 0x61, 0x73, 0x64, 0x66, 0x61, 0x73, 0x64, 0x66 "Mag-nusasdfasdf"
            serverNameFragment.Payload.Write(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x9E, 0x16, 0x14, 0x50, 0x0F, 0x00, 0x4D, 0x61, 0x67, 0x2D, 0x6E, 0x75, 0x73, 0x61, 0x73, 0x64, 0x66, 0x61, 0x73, 0x64, 0x66, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            serverName.Fragments.Add(serverNameFragment);

            NetworkManager.SendLoginPacket(serverName, session);
        }
    }
}
