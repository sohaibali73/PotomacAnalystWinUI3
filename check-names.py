import re

path = r'C:\Users\SohaibAli\Documents\PotomacAnalyst\Views\Pages\AflGeneratorPage.xaml'
with open(path, encoding='utf-8') as f:
    content = f.read()
names = re.findall(r'x:Name="(\w+)"', content)
print('x:Names in AflGeneratorPage.xaml:', sorted(names))
print('Has StrategyType:', 'StrategyType' in names)
print('Has Timeframe:', 'Timeframe' in names)
print('Has Indicators:', 'Indicators' in names)
print('Has PositionSize:', 'PositionSize' in names)
print('Has StopLoss:', 'StopLoss' in names)

path2 = r'C:\Users\SohaibAli\Documents\PotomacAnalyst\MainWindow.xaml'
with open(path2, encoding='utf-8') as f:
    content2 = f.read()
names2 = re.findall(r'x:Name="(\w+)"', content2)
print('\nHas LogoTextPanel in MainWindow.xaml:', 'LogoTextPanel' in names2)
print('MainWindow x:Names:', sorted(names2))
