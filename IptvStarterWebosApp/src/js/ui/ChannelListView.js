export class ChannelListView {
  constructor(containerId) {
    this.container = document.getElementById(containerId);
  }

  render(channels, onSelect) {
    this.container.innerHTML = '';

    channels.forEach((channel) => {
      const button = document.createElement('button');
      button.className = 'channel';
      button.type = 'button';
      button.textContent = channel.name;
      button.addEventListener('click', () => onSelect(channel));
      this.container.appendChild(button);
    });
  }
}
