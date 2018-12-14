import { Get, Post } from '../../utilities/requestHelper';
import * as signalR from '@aspnet/signalr';

export default {
    // loadParameters : { count: int, others... }
    props: ['path', 'loadUrl', 'loadParameters', 'sendUrl', 'sendParameters', 'showSnack'],
    data: () => ({
        chats: [],
        chatLastload: 2147483647,
        inputText: '',
        connection: null,
        msgLoading: true,
        inputRules: [
            v => !!v || '请输入发送内容'
        ],
        currentReply: 0,
        hubConnected: false,
        sending: false
    }),
    mounted: function () {
        let msgList = document.getElementById('msgList');
        if (msgList !== null) {
            this.loadMessages().then(() => {
                msgList.scrollTop = msgList.scrollHeight - msgList.clientHeight;
                msgList.onscroll = () => {
                    if (msgList.scrollTop <= 100) {
                        if (this.chatLastload !== -1) {
                            this.loadMessages(this.chatLastload).then(cnt => {
                                msgList.scrollTop += 157.7 * cnt;
                            });
                        }
                    }
                };
                this.connection = this.buildSingalR();
            });
        }
    },
    destroyed: function () {
        if (this.connection !== null)
            this.connection.stop();
    },
    methods: {
        loadMessages: function (id = 2147483647) {
            if (this.msgLoading) return 0;
            this.msgLoading = true;
            let param = this.loadParameters;
            param['startId'] = id;
            return Get(this.loadUrl, param)
                .then(res => res.json())
                .then(data => {
                    if (data.isSucceeded) {
                        let messages = data.chatMessages;
                        for (var i in messages) {
                            messages[i]['avatar'] = '/Account/GetUserAvatar?userId=' + messages[i]['userId'];
                        }
                        this.chats = messages.concat(this.chats);
                        if (messages.length > 0) {
                            this.chatLastload = messages[0].id;
                        }
                        else {
                            this.chatLastload = -1;
                        }
                        this.msgLoading = false;
                        return messages.length;
                    }
                    else {
                        this.showSnack(data.errorMessage, 'error', 3000);
                        this.msgLoading = false;
                        return 0;
                    }
                })
                .catch(() => {
                    this.showSnack('消息加载失败', 'error', 3000);
                    this.msgLoading = false;
                });
        },
        sendMessage: function () {
            if (this.inputText && this.inputText.length <= 65536)
                this.sending = true;
            let param = this.sendParameters;
            param['content'] = this.inputText;
            param['replyId'] = this.currentReply;
            Post(this.sendUrl, param)
                .then(res => res.json())
                .then(data => {
                    if (!data.isSucceeded) {
                        this.showSnack(data.errorMessage, 'error', 3000);
                    } else {
                        this.showSnack('发送成功，获得 5 经验', 'success', 3000);
                        this.currentReply = 0;
                    }
                    this.sending = false;
                })
                .catch(() => {
                    this.showSnack('发送失败', 'error', 3000);
                    this.sending = false;
                });
        },
        cancelReply: function () {
            this.currentReply = 0;
        },
        processReply: function (id) {
            this.currentReply = id;
        },
        buildSingalR: function () {
            let path = '';
            for (let i in this.path) {
                path += i + '-' + this.path[i] + ';';
            }
            let conn = new signalR.HubConnectionBuilder().withUrl('/ChatHub?path=' + path).build();
            conn.on('ChatMessage', (id, userId, userName, sendTime, content, replyId) => {
                let data = {
                    id: id,
                    userId: userId,
                    userName: userName,
                    sendTime: sendTime,
                    content: content,
                    replyId: replyId,
                    avatar: '/Account/GetUserAvatar?userId=' + userId
                };
                let msgList = document.getElementById('msgList');
                if (msgList !== null) {
                    let dTop = msgList.scrollHeight - msgList.clientHeight - msgList.scrollTop;
                    this.chats = this.chats.concat([data]);
                    if (dTop <= 100 || !!this.inputText) {
                        this.$nextTick(() => {
                            msgList.scrollTop = msgList.scrollHeight - msgList.clientHeight;
                        });
                    }
                }
                this.inputText = '';
            });
            conn.onclose(error => {
                this.hubConnected = false;
                if (error) {
                    this.connection = this.buildSingalR();
                }
            });
            conn.start()
                .then(() => this.hubConnected = true)
                .catch(() => {
                    this.hubConnected = false;
                    this.connection = this.buildSingalR();
                });
            return conn;
        }
    }
};