# Roku

O Roku não é executado em C# Xamarin.Android. Para disponibilizar em Roku, o caminho correto é:

1. Criar um canal Roku usando BrightScript/SceneGraph.
2. Consumir a playlist M3U em um serviço remoto ou em um endpoint JSON.
3. Reaproveitar a mesma regra de negócio e dados do projeto, mas não usar o mesmo código nativo Android.

Em outras palavras, o mesmo backend e as regras de playlist podem ser compartilhados, mas a UI e o runtime do Roku são diferentes.
