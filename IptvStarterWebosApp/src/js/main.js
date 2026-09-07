import { PlaylistService } from './services/PlaylistService.js';
import { ChannelListView } from './ui/ChannelListView.js';

const defaultPlaylistUrl = 'https://example.com/playlist.m3u';
const playlistService = new PlaylistService();
const channelListView = new ChannelListView('channels');
const statusEl = document.getElementById('status');
const videoPlayer = document.getElementById('videoPlayer');

async function loadChannels() {
  try {
    statusEl.textContent = 'Carregando lista de canais...';
    const channels = await playlistService.loadFromUrl(defaultPlaylistUrl);

    if (!channels.length) {
      statusEl.textContent = 'Nenhum canal foi encontrado na playlist.';
      return;
    }

    statusEl.textContent = 'Lista carregada. Use VPN antes do acesso.';
    channelListView.render(channels, (channel) => {
      videoPlayer.src = channel.url;
      videoPlayer.play();
    });
  } catch (error) {
    statusEl.textContent = `Erro ao carregar playlist: ${error.message}`;
  }
}

loadChannels();
