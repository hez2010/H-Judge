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
                    <chatboard :path="{ pid: 0, cid: 0, gid: 0 }"
                               :loadUrl="'/Message/GetChats'" 
                               :loadParameters="{ count: 10 }" 
                               :sendUrl="'/Message/SendChat'"
                               :sendParameters="{}"
                               :showSnack="showSnack">
                    </chatboard>
                </div>
            </v-flex>
        </v-layout>
    </v-container>
</template>

<script type="text/javascript" src="./home.js"></script>