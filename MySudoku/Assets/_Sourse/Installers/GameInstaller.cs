using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Zenject;

using Controllers;
using Core.Board;
using Core.Generation;
using Core.Generation.Strategies;
using Data;
using Presentation.Views;
using Services;
using Signals;

namespace Installers
{
    public class GameInstaller : MonoInstaller
    {
        [Header("Цветовые темы (пока одна, см. ThemeService)")]
        [SerializeField] private List<ColorThemeSO> _availableThemes;

        [Header("Баланс")]
        [SerializeField] private ScoreSettingsSO _scoreSettings;
        [SerializeField] private CurrencyRewardSO _currencyReward;
        [SerializeField] private DifficultyConfigSO _difficultyConfig;

        public override void InstallBindings()
        {
            InstallSignals();
            InstallData();
            InstallScreens();
            InstallCoreGeneration();
            InstallServices();
            InstallControllers();
        }

        // ---------- Signals (Observer) ----------

        private void InstallSignals()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<CellFilledSignal>();
            Container.DeclareSignal<MistakeMadeSignal>();
            Container.DeclareSignal<HealthDepletedSignal>();
            Container.DeclareSignal<ComboChangedSignal>();
            Container.DeclareSignal<ScoreChangedSignal>();
            Container.DeclareSignal<BoardCompletedSignal>();
            Container.DeclareSignal<HintUsedSignal>();
            Container.DeclareSignal<ReturnedToMainMenuSignal>();
            Container.DeclareSignal<GameOverSignal>();
            Container.DeclareSignal<ThemeChangedSignal>();
        }

        // ---------- ScriptableObject-данные ----------

        private void InstallData()
        {
            Container.Bind<IReadOnlyList<ColorThemeSO>>().FromInstance(_availableThemes).AsSingle();
            Container.Bind<ScoreSettingsSO>().FromInstance(_scoreSettings).AsSingle();
            Container.Bind<CurrencyRewardSO>().FromInstance(_currencyReward).AsSingle();
            Container.Bind<DifficultyConfigSO>().FromInstance(_difficultyConfig).AsSingle();
        }

        // ---------- Экраны и попапы (MonoBehaviour со сцены) ----------

        private void InstallScreens()
        {
            // StaticUICanvas — экраны, между которыми переключается ShowExclusive()
            Container.Bind<MainMenuController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<StatisticsController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<SettingsController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<TutorialController>().FromComponentInHierarchy().AsSingle();

            // Нижняя навигация — не экран, всегда на месте
            Container.Bind<NavigationBarController>().FromComponentInHierarchy().AsSingle();

            // Панель выбора сложности — оверлей поверх меню, не экран
            Container.Bind<DifficultySelectController>().FromComponentInHierarchy().AsSingle();

            // GameplayCanvas
            Container.Bind<GameView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<BoardView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<HudView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<NumberPadView>().FromComponentInHierarchy().AsSingle();

            // OverlayCanvas — попапы
            Container.Bind<ConfirmationDialogView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<PauseOverlayView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ChillModeWarningView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<PurchaseOfferController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<GameOverPanelController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<VictoryPanelController>().FromComponentInHierarchy().AsSingle();

            Container.Bind<BackgroundThemeApplier>().FromComponentInHierarchy().AsSingle();

            // Список типов экранов для ScreenService — резолвятся лениво
            // (см. комментарий в ScreenService), не сразу здесь.
            var screenTypes = new List<System.Type>
            {
                typeof(MainMenuController),
                typeof(StatisticsController),
                typeof(SettingsController),
                typeof(TutorialController),
            };

            Container.Bind<IScreenService>()
                .FromMethod(ctx => new ScreenService(ctx.Container, screenTypes))
                .AsSingle();
        }

        // ---------- Core: генерация полей ----------

        private void InstallCoreGeneration()
        {
            Container.Bind<IReadOnlyDictionary<DifficultyLevel, DifficultySettings>>()
                .FromMethod(ctx => ctx.Container.Resolve<DifficultyConfigSO>().BuildSettings())
                .AsSingle();

            Container.Bind<Classic9x9Strategy>().AsSingle();

            // TODO: заменить на реального провайдера, читающего PuzzleBankAsset,
            // когда банк пазлов для 16x16/25x25 будет подготовлен.
            Container.Bind<Core.Generation.PuzzleBank.IPuzzleBankProvider>()
                .To<EmptyPuzzleBankProvider>().AsSingle();

            Container.Bind<Grid16x16Strategy>().AsSingle();
            Container.Bind<Grid25x25Strategy>().AsSingle();

            Container.Bind<IBoardGeneratorFactory>().To<BoardGeneratorFactory>().AsSingle();
        }

        // ---------- Services ----------

        private void InstallServices()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "save.bin");
            string statsPath = Path.Combine(Application.persistentDataPath, "stats.bin");
            string currencyPath = Path.Combine(Application.persistentDataPath, "currency.bin");
            string themePath = Path.Combine(Application.persistentDataPath, "theme.bin");

            Container.BindInterfacesAndSelfTo<CurrencyService>().AsSingle().WithArguments(currencyPath);
            Container.BindInterfacesAndSelfTo<SaveService>().AsSingle().WithArguments(savePath);
            Container.BindInterfacesAndSelfTo<StatsRepository>().AsSingle().WithArguments(statsPath);
            Container.BindInterfacesAndSelfTo<ThemeService>().AsSingle().WithArguments(themePath);

            Container.BindInterfacesAndSelfTo<TimerService>().AsSingle();
            Container.BindInterfacesAndSelfTo<HealthService>().AsSingle();
            Container.BindInterfacesAndSelfTo<ScoreService>().AsSingle();

            Container.Bind<IUndoService>().To<UndoService>().AsSingle();
            Container.Bind<INotesService>().To<NotesService>().AsSingle();
            Container.Bind<IHintService>().To<HintService>().AsSingle();

            // TODO: заменить на реального провайдера поверх LocalizationTableSO,
            // когда таблицы переводов будут готовы (см. обсуждение локализации).
            Container.Bind<ILocalizationProvider>().To<SimpleLocalizationProvider>().AsSingle();
            Container.Bind<ILocalizationService>().To<LocalizationService>().AsSingle();

            Container.BindInterfacesAndSelfTo<GameSessionController>().AsSingle();
        }

        // ---------- Controllers (плюс GameController — единственный НЕ MonoBehaviour) ----------

        private void InstallControllers()
        {
            Container.BindInterfacesAndSelfTo<GameController>().AsSingle();
        }
    }
}
