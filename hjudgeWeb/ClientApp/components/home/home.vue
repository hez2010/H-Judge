<template>
    <v-container fluid>
        <v-layout pa-1>
            <v-img src="https://cdn.hjudge.com/hjudge/assets/banner.jpg" aspect-ratio="6">
                <v-layout justify-center align-center fill-height class="white--text">
                    <div class="headline">欢迎来到 H::Judge！</div>
                </v-layout>
            </v-img>
        </v-layout>
        <v-layout wrap v-bind="isColumn">
            <v-flex xs4 pa-1>
                <v-toolbar color="secondary" dark>
                    <v-toolbar-title>公告板</v-toolbar-title>
                    <v-spacer></v-spacer>
                    <v-icon>info</v-icon>
                </v-toolbar>
                <div style="height: 550px; overflow: auto">
                    <v-expansion-panel focusable>
                        <v-layout full-height justify-center align-center style="height: 550px" v-if="annLoading">
                            正在加载...
                        </v-layout>
                        <v-layout full-height justify-center align-center style="height: 550px" v-else-if="announcements.length == 0">没有数据 :(</v-layout>
                        <v-expansion-panel-content v-for="(item, i) in announcements"
                                                   :key="i"
                                                   v-else>
                            <div slot="header">{{item.title}}</div>
                            <v-container v-html="item.content"></v-container>
                        </v-expansion-panel-content>
                    </v-expansion-panel>
                </div>
            </v-flex>
            <v-flex xs8 pa-1>
                <v-toolbar color="secondary" dark>
                    <v-icon>message</v-icon>
                    <v-spacer></v-spacer>
                    <v-toolbar-title>版聊区</v-toolbar-title>
                </v-toolbar>
                <div style="height: 550px">
                    <v-list style="height: 420px; overflow: auto" id="msgList">
                        <v-layout full-height justify-center align-center style="height: 410px" v-if="msgLoading">
                            正在加载...
                        </v-layout>
                        <v-layout full-height justify-center align-center style="height: 410px" v-else-if="chats.length == 0">没有数据 :(</v-layout>
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
                                    <v-list-tile-sub-title>{{item.sendTime}} #{{item.id}} {{item.replyId !== 0 ? '回复 #' + item.replyId : ''}}</v-list-tile-sub-title>
                                </v-list-tile-content>
                            </v-list-tile>
                            <v-container style="width: 90%; overflow: auto; height: 100px;">
                                <pre v-html="item.content"></pre>
                            </v-container>
                            <v-divider v-if="index + 1 < chats.length" :key="`divider-${index}`"></v-divider>
                        </div>
                    </v-list>
                    <v-layout>
                        <v-flex xs11>
                            <v-textarea :label="`${'输入你要发送的内容' + (currentReply === 0 ? '（点击头像进行回复）' : '（回复 #' + currentReply + '）')}`" v-model="inputText" hint="资费：10 金币/条"></v-textarea>
                        </v-flex>
                        <v-flex>
                            <v-tooltip bottom>
                                <v-btn icon slot="activator" @click="sendMessage" :disabled="!inputText">
                                    <v-icon>send</v-icon>
                                </v-btn>
                                <span>发送</span>
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
            </v-flex>
        </v-layout>
    </v-container>
</template>

<script type="text/javascript" src="./home.js"></script>