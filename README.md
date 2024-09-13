# mensabot
A discord "bot" that sends the current TU KL mensa menu to a channel via webhook.
Uses the www.mensa-kl.de API.

## How to use
- Create a webhook on your Discord server
- Edit `config.json` and add that webhook

Executing `dotnet run` will now send a message with tomorrow's menu to the webhook's channel.

An example systemD service and timer config is included.
Note that `mensabot.service` will need to be edited before use.
