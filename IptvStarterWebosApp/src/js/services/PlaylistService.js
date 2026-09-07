export class PlaylistService {
  async loadFromUrl(url) {
    const response = await fetch(url);
    if (!response.ok) {
      throw new Error(`Erro ao carregar playlist: ${response.status}`);
    }

    const content = await response.text();
    return this.parseM3U(content);
  }

  parseM3U(content) {
    const lines = content.split(/\r?\n/).filter(Boolean);
    const channels = [];
    let currentChannel = null;

    for (const line of lines) {
      if (line.startsWith('#EXTINF')) {
        currentChannel = { name: line.split(',').slice(1).join(',').trim() || 'Canal IPTV' };
        const groupMatch = /group-title="([^"]+)"/i.exec(line);
        if (groupMatch) {
          currentChannel.group = groupMatch[1];
        }
        continue;
      }

      if (line.startsWith('#')) {
        continue;
      }

      if (currentChannel) {
        channels.push({
          id: crypto.randomUUID(),
          name: currentChannel.name,
          url: line,
          group: currentChannel.group || 'General'
        });
        currentChannel = null;
      }
    }

    return channels;
  }
}
