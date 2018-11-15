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
                        <v-expansion-panel-content v-for="(item, i) in announcements"
                                                   :key="i">
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
                    <v-list style="height: 430px; overflow: auto" id="msgList">
                        <div v-for="(item, index) in chats"
                             :key="item.title">
                            <v-list-tile avatar>
                                <v-list-tile-avatar>
                                    <img :src="item.avatar">
                                </v-list-tile-avatar>
                                <v-list-tile-content>
                                    <router-link :to="{ path: '/Account/' + item.userId }">
                                        <v-list-tile-title>{{item.userName}}</v-list-tile-title>
                                    </router-link>
                                    <v-list-tile-sub-title>{{item.sendTime}}</v-list-tile-sub-title>
                                </v-list-tile-content>
                            </v-list-tile>
                            <v-container style="width: 90%; overflow: auto; max-height: 100px;" v-html="item.content"></v-container>
                            <v-divider v-if="index + 1 < chats.length" :key="`divider-${index}`"></v-divider>
                        </div>
                    </v-list>
                    <v-layout>
                        <v-textarea label="输入你要发送的内容" v-model="inputText"></v-textarea>
                        <v-tooltip bottom>
                            <v-btn icon slot="activator" @click="sendMessage">
                                <v-icon>send</v-icon>
                            </v-btn>
                            <span>发送</span>
                        </v-tooltip>
                    </v-layout>
                </div>
            </v-flex>
        </v-layout>
    </v-container>
</template>

<script type="text/javascript" src="./home.js"></script>