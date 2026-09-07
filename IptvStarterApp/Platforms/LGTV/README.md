# LG webOS

Para LG webOS, a aplicação é desenvolvida em JavaScript/TypeScript com framework web oficial do webOS e o runtime do aparelho.

Estratégia recomendada:

1. Expor uma API em .NET para a lista de canais eURLs M3U.
2. Criar a app web para webOS consumindo essa API.
3. Reaproveitar os modelos de dados e o parser M3U.

O código Android do Xamarin não roda diretamente em webOS/LG TV.
