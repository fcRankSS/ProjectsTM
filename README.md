# Introduction 
TODO: Give a short introduction of your project. Let this section explain the objectives or the motivation behind this project. 

# Getting Started
TODO: Guide users through getting your code up and running on their own system. In this section you can talk about:
1.	Installation process
2.	Software dependencies
3.	Latest releases
4.	API references

# Build and Test
TODO: Describe and show how to build your code and run the tests. 

# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 

If you want to learn more about creating good readme files then refer the following [guidelines](https://www.visualstudio.com/en-us/docs/git/create-a-readme). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)


# slack

app���쐬�B
Permition Scope��ݒ�B
�C���X�g�[���B���g�[�N��(xoxp-xxxx)�������B
�g�[�N���ƃ`���l�����w�肵�ā�����œ��e�B
https://slack.com/api/chat.postMessage?token=xoxp-xxxx&channel=aa&text=%22Hello%22

https://slack.com/api/chat.postMessage?token=xoxp-427935500258-429278393766-628991580148-1e77c82043c4a4e851749c7ead55df8a&channel=aa&text=%22Hello%22

Invoke-WebRequest -Method Post -Body @{token="xoxp-427935500258-429278393766-628991580148-1e77c82043c4a4e851749c7ead55df8a"; channel="aa"; text="Good night."} https://slack.com/api/chat.postMessage



karNI8bSRoJXMRLgcqyWUNhbm78EUkZnac2bQXn3YlM

curl -X POST -H "Authorization: Bearer ACCESS_TOKEN" -F "message=ABC" https://notify-api.line.me/api/notify

CIIF
Invoke-WebRequest -Method Post -Headers @{Authorization="Bearer karNI8bSRoJXMRLgcqyWUNhbm78EUkZnac2bQXn3YlM"} -Body @{message="Good night."} https://notify-api.line.me/api/notify

�Ă���
Invoke-WebRequest -Method Post -Headers @{Authorization="Bearer rHaV0FNRAg3WuSmYYhiqIS1ydZpr9YDtBAW72utZwUK"} -Body @{message="�͂����."} https://notify-api.line.me/api/notify
