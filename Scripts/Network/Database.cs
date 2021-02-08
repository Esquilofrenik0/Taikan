using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System.IO;

namespace Postcarbon {
  public class Database: NetworkedBehaviour {
    public List<dItem> database;
    public List<dRecipe> recipes;

    public dItem GetItem(string _name) {
      for (int i = 0; i < database.Count; i++) {
        if (database[i].Name == _name) {
          return Instantiate(database[i]);
        }
      }
      return null;
    }

    void Awake() {
      Application.targetFrameRate = 60;
      write_dItem();
      write_dBuildable();
      write_dConsumable();
      write_dScrap();
      write_dRecipe();
      write_dArmor();
      write_dWeapon();
      write_dMelee();
      write_dGun();
      write_dShield();
      write_dAmmo();
    }

    #region dItem
    void write_dItem() {
      SerializationManager.RegisterSerializationHandlers<dItem>((Stream stream, dItem instance) => {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream)) {
          writer.WriteStringPacked(instance.Name);
          writer.WriteDoublePacked(instance.value);
          writer.WriteDoublePacked(instance.weight);
          if (instance is dArmor) {
            dArmor dArmor = (dArmor)instance;
            writer.WriteDoublePacked(dArmor.defense);
            writer.WriteDoublePacked(dArmor.durability);
          }
          else if (instance is dWeapon) {
            dWeapon dWeapon = (dWeapon)instance;
            writer.WriteDoublePacked(dWeapon.damage);
            writer.WriteDoublePacked(dWeapon.durability);
            if (dWeapon is dShield) {
              dShield dShield = (dShield)dWeapon;
              writer.WriteDoublePacked(dShield.defense);
            }
            else if (dWeapon is dGun) {
              dGun dGun = (dGun)dWeapon;
              writer.WriteInt32Packed(dGun.clipSize);
              writer.WriteInt32Packed(dGun.clipAmmo);
              writer.WriteInt32Packed(dGun.totalAmmo);
            }
          }
        }
      },
      (Stream stream) => {
        using (PooledBitReader reader = PooledBitReader.Get(stream)) {
          dItem dItem = GetItem(reader.ReadStringPacked().ToString());
          dItem.value = (float)reader.ReadDoublePacked();
          dItem.weight = (float)reader.ReadDoublePacked();
          if (dItem is dArmor) {
            dArmor dArmor = (dArmor)dItem;
            dArmor.defense = (float)reader.ReadDoublePacked();
            dArmor.durability = (float)reader.ReadDoublePacked();
            dItem = (dItem)dArmor;
          }
          else if (dItem is dWeapon) {
            dWeapon dWeapon = (dWeapon)dItem;
            dWeapon.damage = (float)reader.ReadDoublePacked();
            dWeapon.durability = (float)reader.ReadDoublePacked();
            if (dWeapon is dShield) {
              dShield dShield = (dShield)dWeapon;
              dShield.defense = (float)reader.ReadDoublePacked();
              dWeapon = (dWeapon)dShield;
            }
            else if (dWeapon is dGun) {
              dGun dGun = (dGun)dWeapon;
              dGun.clipSize = reader.ReadInt32Packed();
              dGun.clipAmmo = reader.ReadInt32Packed();
              dGun.totalAmmo = reader.ReadInt32Packed();
              dWeapon = (dWeapon)dGun;
            }
            dItem = (dItem)dWeapon;
          }
          return dItem;
        }
      });
    }

    void write_dBuildable() {
      SerializationManager.RegisterSerializationHandlers<dBuildable>((Stream stream, dBuildable instance) => {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream)) {
          writer.WriteStringPacked(instance.Name);
          writer.WriteDoublePacked(instance.value);
          writer.WriteDoublePacked(instance.weight);
        }
      },
      (Stream stream) => {
        using (PooledBitReader reader = PooledBitReader.Get(stream)) {
          dBuildable dBuildable = (dBuildable)GetItem(reader.ReadStringPacked().ToString());
          dBuildable.value = (float)reader.ReadDoublePacked();
          dBuildable.weight = (float)reader.ReadDoublePacked();
          return dBuildable;
        }
      });
    }

    void write_dConsumable() {
      SerializationManager.RegisterSerializationHandlers<dConsumable>((Stream stream, dConsumable instance) => {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream)) {
          writer.WriteStringPacked(instance.Name);
          writer.WriteDoublePacked(instance.value);
          writer.WriteDoublePacked(instance.weight);
        }
      },
      (Stream stream) => {
        using (PooledBitReader reader = PooledBitReader.Get(stream)) {
          dConsumable dConsumable = (dConsumable)GetItem(reader.ReadStringPacked().ToString());
          dConsumable.value = (float)reader.ReadDoublePacked();
          dConsumable.weight = (float)reader.ReadDoublePacked();
          return dConsumable;
        }
      });
    }

    void write_dScrap() {
      SerializationManager.RegisterSerializationHandlers<dScrap>((Stream stream, dScrap instance) => {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream)) {
          writer.WriteStringPacked(instance.Name);
          writer.WriteDoublePacked(instance.value);
          writer.WriteDoublePacked(instance.weight);
        }
      },
      (Stream stream) => {
        using (PooledBitReader reader = PooledBitReader.Get(stream)) {
          dScrap dScrap = (dScrap)GetItem(reader.ReadStringPacked().ToString());
          dScrap.value = (float)reader.ReadDoublePacked();
          dScrap.weight = (float)reader.ReadDoublePacked();
          return dScrap;
        }
      });
    }

    void write_dAmmo() {
      SerializationManager.RegisterSerializationHandlers<dAmmo>((Stream stream, dAmmo instance) => {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream)) {
          writer.WriteStringPacked(instance.Name);
          writer.WriteDoublePacked(instance.value);
          writer.WriteDoublePacked(instance.weight);
        }
      },
      (Stream stream) => {
        using (PooledBitReader reader = PooledBitReader.Get(stream)) {
          dAmmo dAmmo = (dAmmo)GetItem(reader.ReadStringPacked().ToString());
          dAmmo.value = (float)reader.ReadDoublePacked();
          dAmmo.weight = (float)reader.ReadDoublePacked();
          return dAmmo;
        }
      });
    }


    void write_dRecipe() {
      SerializationManager.RegisterSerializationHandlers<dRecipe>((Stream stream, dRecipe instance) => {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream)) {
          writer.WriteStringPacked(instance.Name);
          writer.WriteDoublePacked(instance.value);
          writer.WriteDoublePacked(instance.weight);
        }
      },
      (Stream stream) => {
        using (PooledBitReader reader = PooledBitReader.Get(stream)) {
          dRecipe dRecipe = (dRecipe)GetItem(reader.ReadStringPacked().ToString());
          dRecipe.value = (float)reader.ReadDoublePacked();
          dRecipe.weight = (float)reader.ReadDoublePacked();
          return dRecipe;
        }
      });
    }

    void write_dArmor() {
      SerializationManager.RegisterSerializationHandlers<dArmor>((Stream stream, dArmor instance) => {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream)) {
          writer.WriteStringPacked(instance.Name);
          writer.WriteDoublePacked(instance.value);
          writer.WriteDoublePacked(instance.weight);
          writer.WriteDoublePacked(instance.defense);
          writer.WriteDoublePacked(instance.durability);
        }
      },
      (Stream stream) => {
        using (PooledBitReader reader = PooledBitReader.Get(stream)) {
          dArmor dArmor = (dArmor)GetItem(reader.ReadStringPacked().ToString());
          dArmor.value = (float)reader.ReadDoublePacked();
          dArmor.weight = (float)reader.ReadDoublePacked();
          dArmor.defense = (float)reader.ReadDoublePacked();
          dArmor.durability = (float)reader.ReadDoublePacked();
          return dArmor;
        }
      });
    }

    void write_dWeapon() {
      SerializationManager.RegisterSerializationHandlers<dWeapon>((Stream stream, dWeapon instance) => {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream)) {
          writer.WriteStringPacked(instance.Name);
          writer.WriteDoublePacked(instance.value);
          writer.WriteDoublePacked(instance.weight);
          writer.WriteDoublePacked(instance.damage);
          writer.WriteDoublePacked(instance.durability);
          if (instance is dShield) {
            dShield dShield = (dShield)instance;
            writer.WriteDoublePacked(dShield.defense);
          }
          else if (instance is dGun) {
            dGun dGun = (dGun)instance;
            writer.WriteInt32Packed(dGun.clipSize);
            writer.WriteInt32Packed(dGun.clipAmmo);
            writer.WriteInt32Packed(dGun.totalAmmo);
          }
        }
      },
      (Stream stream) => {
        using (PooledBitReader reader = PooledBitReader.Get(stream)) {
          dWeapon dWeapon = (dWeapon)GetItem(reader.ReadStringPacked().ToString());
          dWeapon.value = (float)reader.ReadDoublePacked();
          dWeapon.weight = (float)reader.ReadDoublePacked();
          dWeapon.damage = (float)reader.ReadDoublePacked();
          dWeapon.durability = (float)reader.ReadDoublePacked();
          if (dWeapon is dShield) {
            dShield dShield = (dShield)dWeapon;
            dShield.defense = (float)reader.ReadDoublePacked();
            dWeapon = (dWeapon)dShield;
          }
          else if (dWeapon is dGun) {
              dGun dGun = (dGun)dWeapon;
              dGun.clipSize = reader.ReadInt32Packed();
              dGun.clipAmmo = reader.ReadInt32Packed();
              dGun.totalAmmo = reader.ReadInt32Packed();
              dWeapon = (dWeapon)dGun;
            }
          return dWeapon;
        }
      });
    }

    void write_dMelee() {
      SerializationManager.RegisterSerializationHandlers<dMelee>((Stream stream, dMelee instance) => {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream)) {
          writer.WriteStringPacked(instance.Name);
          writer.WriteDoublePacked(instance.value);
          writer.WriteDoublePacked(instance.weight);
          writer.WriteDoublePacked(instance.damage);
          writer.WriteDoublePacked(instance.durability);
        }
      },
      (Stream stream) => {
        using (PooledBitReader reader = PooledBitReader.Get(stream)) {
          dMelee dMelee = (dMelee)GetItem(reader.ReadStringPacked().ToString());
          dMelee.value = (float)reader.ReadDoublePacked();
          dMelee.weight = (float)reader.ReadDoublePacked();
          dMelee.damage = (float)reader.ReadDoublePacked();
          dMelee.durability = (float)reader.ReadDoublePacked();
          return dMelee;
        }
      });
    }

    void write_dGun() {
      SerializationManager.RegisterSerializationHandlers<dGun>((Stream stream, dGun instance) => {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream)) {
          writer.WriteStringPacked(instance.Name);
          writer.WriteDoublePacked(instance.value);
          writer.WriteDoublePacked(instance.weight);
          writer.WriteDoublePacked(instance.damage);
          writer.WriteDoublePacked(instance.durability);
          writer.WriteInt32Packed(instance.clipSize);
          writer.WriteInt32Packed(instance.clipAmmo);
          writer.WriteInt32Packed(instance.totalAmmo);
        }
      },
      (Stream stream) => {
        using (PooledBitReader reader = PooledBitReader.Get(stream)) {
          dGun dGun = (dGun)GetItem(reader.ReadStringPacked().ToString());
          dGun.value = (float)reader.ReadDoublePacked();
          dGun.weight = (float)reader.ReadDoublePacked();
          dGun.damage = (float)reader.ReadDoublePacked();
          dGun.durability = (float)reader.ReadDoublePacked();
          dGun.clipSize = reader.ReadInt32Packed();
          dGun.clipAmmo = reader.ReadInt32Packed();
          dGun.totalAmmo = reader.ReadInt32Packed();
          return dGun;
        }
      });
    }

    void write_dShield() {
      SerializationManager.RegisterSerializationHandlers<dShield>((Stream stream, dShield instance) => {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream)) {
          writer.WriteStringPacked(instance.Name);
          writer.WriteDoublePacked(instance.value);
          writer.WriteDoublePacked(instance.weight);
          writer.WriteDoublePacked(instance.damage);
          writer.WriteDoublePacked(instance.durability);
          writer.WriteDoublePacked(instance.defense);
        }
      },
      (Stream stream) => {
        using (PooledBitReader reader = PooledBitReader.Get(stream)) {
          dShield dShield = (dShield)GetItem(reader.ReadStringPacked().ToString());
          dShield.value = (float)reader.ReadDoublePacked();
          dShield.weight = (float)reader.ReadDoublePacked();
          dShield.damage = (float)reader.ReadDoublePacked();
          dShield.durability = (float)reader.ReadDoublePacked();
          dShield.defense = (float)reader.ReadDoublePacked();
          return dShield;
        }
      });
    }
    #endregion
  }
}
