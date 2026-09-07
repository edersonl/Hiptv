# Hiptv

Aplicativo base para IPTV com visual em preto, vermelho e branco, focado em canais ao vivo, filmes, séries e favoritos.

## Nome do projeto

- App: Hiptv
- Repositório GitHub: https://github.com/edersonl/Hiptv.git

## Objetivo

- exibir canais ao vivo
- listar filmes e séries
- permitir favoritos
- reproduzir streams por URL
- manter arquitetura separada em UI, serviço e modelos

## Estrutura principal

- `Config/`: informações do app e links
- `Models/`: modelo de canais e itens
- `Services/`: carregamento da playlist M3U
- `UI/`: telas do app Android
- `Resources/`: layouts e estilos visuais

## Personalização visual

A identidade visual atual foi ajustada para o padrão:

- preto
- vermelho
- branco

Também já foi pensado no layout principal com categorias:

- Canais ao Vivo
- Filmes
- Séries
- Favoritos

## Como rodar

1. Instale o SDK do .NET 8 e o workload do Android.
2. Abra a pasta do projeto no Visual Studio ou em um ambiente com Xamarin.Android.
3. Ajuste a URL da playlist M3U em `Config/AppConfig.cs`.
4. Compile e execute o projeto em emulador ou dispositivo Android.

## Git

Este projeto foi preparado para versionamento com Git e com o repositório remoto configurado no padrão:

```bash
git init
git branch -M main
git remote add origin https://github.com/edersonl/Hiptv.git
```

## Observação

Use VPN antes de acessar streams, especialmente quando o provedor ou o conteúdo tiver restrições geográficas.
