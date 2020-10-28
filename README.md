# mensabot
A discord "bot" that sends the current TU KL mensa menu to a channel via webhook

## How to use
- Create a `webhooks` file.
- Create a webhook and paste its link into the webhooks file.
Executing `run.sh` will now send a message with tomorrow's menu to the webhook's channel.
Use crontab or some other method to automatically execute `run.sh`.
The webhooks file may contain any amount of webhooks. Blank lines or lines starting with '#' are ignored.
