# Samsung Tizen / Smart TV

Para Samsung Tizen, o app precisa ser desenvolvido com Tizen Web app ou Tizen Native app, não com Xamarin.Android.

Estratégia recomendada:

1. Manter uma API/serviço em .NET para expor a playlist e os canais.
2. Criar uma app web para Tizen ou um app nativo Tizen que consuma essa API.
3. Reaproveitar a lógica de parsing M3U e o modelo de dados.

A lógica do servidor e do parser pode ser compartilhada, mas a UI não pode ser a mesma do Android.
