using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillMakingNote : MonoBehaviour
{


    /*
     スキル追加する時の流れ・やり方
    0.必要なオブジェクト: SkillModel, SkillCaster , スキルの移動方を決めるクラス(例: SkillForwardMove, SkillCircleMove...)、当たり判定...etc
    1.SkillEnum でスキルの種類を定義する, 例: Slash, CircleBall, Tornado, Thunder, PoisonField　の後で　Ice,Arrow...etc
    2.HierarchyのCasterフォルダ内で新GameObjectを作成する, 名前は何々SkillCasterなになに (SkillCasterArrow, SkillCasterIceMagic ...),
    3、新しいスクリプトSkillCasterなになにを作成し、SkillCasterBaseクラスを継承する。先のCasterオブジェクトにSkillCasterなになにスクリプトをアタッチする
    4.Casterから発射するスキルの本体モデル(SkillModel)を作成する,作成したスキル本体はPrefab化してSkillModelエフェクトフォルダ内に置く、そしてCasterのスクリプトのSkillPrefabにアタッチする
    5.スキル本体が必要なComponent(見た目、ステータス、当たり判定、行為(移動など)): 当たり判定用のRigidbody,Box/Sphere Collider(isTriggerをtrueに)、エフェクトオブジェクト(Particle System)、必要なスクリプト(6に)
    6スキル本体に必要なスクリプトをアタッチする, 例: スキルのステータスと種類を決めるSkillModelなになに(SKillModelBaseクラスを継承する) と  スキルの移動方法を決めるスクリプト(例: SkillForwardMove, SkillCircleMove...), 
    7.大体のスキル本体はParticleエフェクトとか3Dモデルとかがあるので、Particleエフェクトもしくはスキルの3Dモデルを子供に追加する
    8.CasterのCooldownが0の時に、スキル本体のCastSkill関数を呼び出し、スキルを発動する。

    例えば、Stoneというスキルを追加する場合は、以下のような流れになります。
    1.SkillEnumクラスのSkillIdTypeで：[JPName("石")]　Stone, でenumを追加,
    2:SkillCasterStoneスクリプトを作成する, これはSkillCasterBaseを継承して、SkillCasterBaseのCastSkill()関数をOverrideする。これは石のスキルを発動するためのCasterスクリプトです。
    3.Casterフォルダ内で新GameObjectを作成する, 名前は同じSkillCasterStone,overrideでHandleSkillInit関数とHandleSkillEndActionをそれぞれ実装する。そして、SkillCasterStoneにSkillCasterStoneスクリプトをアタッチする,
    4.Insepctor内でSkillCasterStoneスクリプトの細かい設定を行う,e.g casterIdType == SkillIdType.Stone, CasterName = 石, ステイタス基礎値を設定：cast CoolDownMax = 3f(最初は3秒一回で発射する),Damage Base = 50f,SkillProjetileNumMax, etc...
    5.SkillCasterStoneスクリプト内でSkillObjectPool(スキルのオブジェクトプール) と casterspriteImage(スキルのアイコン画像)を付ける必要があります。それそれ準備します。
    6.フォルダ: 08_ObjectPool/SkillPool内で　右クリックしてCreate -> MasterObjectPooler2 -> ObjectPoolでプールを作成します。名前は大体"SkillStonePool"とかにします。
    7.次はスキル本体のPrefabを作成します：任意なところで新GameObject作成し、名前は"SkillStoneModel"とか、SkillModelStoneスクリプトを付けます。
    8.そして、見た目となるエフェクト(ParticleSystem)を付けます。03_Effectフォルダから欲しいエフェクト素材(3Dモデルでもいけます)を探して、SkillStoneObjectの子供に追加します。例えば、石のparticleエフェクトを探して、SkillStoneObjectの子供に追加します。
    9.SkillModel内の細かい設定を行います。：設定は最初は(SKillIdType,EffectObjPoolとPsの三つで大丈夫だと思います)　SKillIdType = Stone、EffectObjPoolは先作ったSkillStonePoolをドラグします。　SkillModelのPsのところでこのエフェクトをドラグします。
    10.SkillStoneObjectの設定を完了したら、Prefab化して、02_Prefab/Skill/SkillModel　フォルダに保存します。名前は"SkillStoneModel"とかにします。
    11.ここで大体完成です。SkillStoneCasterのisActivated フラグをtrueにして、スキルが自動に発動できるはず。テストします。
    12.エフェクトが画面に出たけど、スキルの移動方法がないので(SkillCircleMoveとか、SkillForwardMoveとか)、スキルが画面に出たままです。スキルの移動方法を決めるスクリプトを作成します。
    13.移動方法はそれぞれを考えてこのscriptを書きます。
    14.例えば、SkillForwardMoveスクリプトを作成して、transform.position * moveVec * Time.deltaTimedで簡単な直線移動　

    //Check List
    SkillModelオブジェクトに：
    1.SkillModelXXX スクリプトをアタッチする
    2.SkillModelXXX がSkillModelBaseを継承していることを確認する、protected overrideでHandleSkillInit()とHandleSkillEndAction()とHandleSkillOnHitAction()を実装する
    2.Rigidbodyをアタッチする, RigidbodyのUse Gravityはfalseにする
    3.付けたColliderの大きさとエフェクトの大きさを合わせる必要があります。当たり判定を正しくするために。
    3.Colliderをアタッチする, ColliderのisTriggerはtrueにする
    4.Particle Systemをアタッチする, 
    5.SkillObjectPoolをアタッチする, これはスキルのオブジェクトプールです。スキル本体のPrefabを入れる必要があります。
     
    SkillCasterオブジェクトに：
    1.SkillCasterXXX スクリプトをアタッチする
    2. SkillObjectPoolをアタッチする, これはスキルのオブジェクトプールです。スキル本体のPrefabを入れる必要があります。
    3. SkillCasterXXXがSkillCasterBaseを継承していることを確認する、protected overrideでCastSkill()を実装する
    3. casterspriteImageをアタッチする, これはスキルのアイコン画像です。LevelUp UIのspriteに使います。
    3. casterIdType, casterLevelMax,casterName,SkillDesscription, SkilDescriptionFinal,CoolDownFactor,CastCoolDownMax,DurationBase,SpeedBase,DamageBase,SizeBase,ProjectileNumBase,projectileNUmMax の設定を行う

    SkillObjectPoolに：
    autoInitializeをtrueにする,プール自動に初期化する
    autoIncresementをtrueにする。

    SkillCasterとSkillModelのPrefabがoverrideしたかどうか


    P.S:スキル本体(SkillModel)オブジェクトに付けたRigidbodyとColliderの細かい設定を注意する必要があります。

//==============================================================================================================//
 この後は作ったスキルをSkillManagerのスキルリストに追加して、レベルアップする時強化できるような処理についての説明

 1.作成したSkillCasterStoneをSkillManagerのActivateSkilLCollections と　AllSkillCollectionsのリストの中に にドラグする
 2.一度実行してSkillManagerのリストを更新する  
 3.スキル設定をのところで、レベルアップたびなにが強化できるかを設定する。(例: Damage, Speed, Duration, Size, CoolDown, etc...)（副作用も設定できる）
     
    
 * */


}

