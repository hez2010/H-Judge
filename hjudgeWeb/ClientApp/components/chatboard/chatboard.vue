<template>
    <div style="height: 100%">
        <v-list id="msgList" style="overflow: auto; height: calc(100% - 120px); position: relative;">
            <v-layout justify-center align-center style="vertical-align: middle; height: 100%" v-if="msgLoading && chats.length == 0">
                正在加载...
            </v-layout>
            <v-layout justify-center align-center style="vertical-align: middle; height: 100%" v-else-if="chats.length == 0">没有数据 :(</v-layout>
            <div v-for="(item, index) in chats"
                 :key="item.title"
                 v-else>
                <v-list-tile avatar>
                    <v-list-tile-avatar>
                        <img :src="item.avatar" @click="processReply(item.id)" style="cursor: pointer">
                    </v-list-tile-avatar>
                    <v-list-tile-content>
                        <router-link :to="{ path: '/Account/' + item.userId }">
                            <v-list-tile-title>{{item.userName}}</v-list-tile-title>
                        </router-link>
                        <v-list-tile-sub-title>#{{item.id}} {{item.replyId !== 0 ? '回复 #' + item.replyId : ''}} {{item.sendTime}}</v-list-tile-sub-title>
                    </v-list-tile-content>
                </v-list-tile>
                <v-container style="width: 90%; overflow: auto; height: 100px;">
                    <pre style="white-space: pre-wrap; word-wrap: break-word;" v-html="item.content"></pre>
                </v-container>
                <v-divider v-if="index + 1 < chats.length" :key="`divider-${index}`"></v-divider>
            </div>
        </v-list>
        <v-layout style="height: 120px">
            <v-flex xs11>
                <v-textarea :label="`${'输入消息内容' + (currentReply === 0 ? '（可点击头像回复）' : '（回复 #' + currentReply + '）')}`" v-model="inputText" :counter="65536" hint="资费：10 金币/条"></v-textarea>
            </v-flex>
            <v-flex>
                <v-tooltip bottom>
                    <v-btn icon slot="activator" @click="sendMessage" :disabled="!inputText || inputText.length > 65536 || !hubConnected || sending">
                        <v-icon color="primary">send</v-icon>
                    </v-btn>
                    <span v-if="hubConnected && !sending">发送</span>
                    <span v-else-if="!hubConnected">正在连接服务器</span>
                    <span v-else>发送中</span>
                </v-tooltip>
                <v-spacer></v-spacer>
                <v-tooltip bottom v-if="currentReply !== 0">
                    <v-btn icon slot="activator" @click="cancelReply">
                        <v-icon>cancel</v-icon>
                    </v-btn>
                    <span>取消回复</span>
                </v-tooltip>
            </v-flex>
        </v-layout>
    </div>
</template>

<script src="./chatboard.js"></script>