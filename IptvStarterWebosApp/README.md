# IPTV Starter para LG webOS

Este projeto é a base de um app IPTV para LG webOS, com UI em JavaScript e reprodução via HTML5 video.

## Objetivo

- listar canais IPTV a partir de uma playlist M3U
- mostrar aviso para usar VPN
- reproduzir o stream em um player HTML5
- manter a estrutura separada entre UI, serviços e modelos

## Estrutura

- `src/index.html`: tela inicial
- `src/js/main.js`: ponto de entrada
- `src/js/services/PlaylistService.js`: parsing da playlist
- `src/js/ui/ChannelListView.js`: render da lista
- `src/js/models/ChannelItem.js`: modelo do canal
- `server.js`: servidor local de exemplo

## Como rodar

1. Instale o Node.js.
2. No terminal, execute:

```bash
npm install
node server.js
```

3. Abra no navegador:

```text
http://localhost:3000
```

## Observação

Este projeto é o starter webOS. Para a TV LG real, é preciso empacotar para o ambiente webOS e publicar via SDK do fabricante.
