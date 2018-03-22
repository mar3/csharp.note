# CoreTweet

```
	...

    var tokens = CoreTweet.Tokens.Create(
        "xxxxxxxxxxxxxxxxxxxxxxxxx",
        "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
        "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
        "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
    text = text + "テスト #hashtag";
    tokens.Statuses.Update(new { status = text });
```
