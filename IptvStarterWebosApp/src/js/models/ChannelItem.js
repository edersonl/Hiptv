export class ChannelItem {
  constructor({ id, name, url, group = 'General' }) {
    this.id = id;
    this.name = name;
    this.url = url;
    this.group = group;
  }
}
