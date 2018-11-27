<template>
    <v-app :dark="darkTheme">
        <navmenu ref="navmenu" :themeIcon="themeIcon" :switchTheme="switchTheme" :user="userInfo" :getUserInfo="getUserInfo" :showMsg="showMsg" :showSnack="showSnack"></navmenu>
        <v-content>
            <router-view :user="userInfo" :getUserInfo="getUserInfo" :updateFortune="updateFortune" :showMsg="showMsg" :showSnack="showSnack" :isDarkTheme="darkTheme" :setMessageCount="setMessageCount" :updateMessageCount="getMessageCount"></router-view>
        </v-content>
        <v-footer class="pa-3 primary darken-2">
            <v-spacer></v-spacer>
            <span class="white--text ml-3">H::Judge {{ new Date().getFullYear() }}</span>
        </v-footer>
        <v-bottom-sheet inset v-model="msgUp">
            <v-card tile>
                <v-list>
                    <v-list-tile>
                        <v-list-tile-content>
                            <v-list-tile-title>{{msgTitle}}</v-list-tile-title>
                            <v-list-tile-sub-title>{{msgContent}}</v-list-tile-sub-title>
                        </v-list-tile-content>

                        <v-spacer></v-spacer>

                        <v-list-tile-action>
                            <v-tooltip top>
                                <v-btn icon slot="activator" @click="() => this.msgUp = false">
                                    <v-icon>close</v-icon>
                                </v-btn>
                                <span>关闭</span>
                            </v-tooltip>
                        </v-list-tile-action>
                    </v-list-tile>
                </v-list>
            </v-card>
        </v-bottom-sheet>
        <v-snackbar v-model="snackUp"
                    right
                    bottom
                    :color="snackColor"
                    :timeout="snackTimeout">
            {{snackContent}}
            <v-tooltip>
                <v-btn dark
                       flat
                       icon
                       slot="activator"
                       @click="() => this.snackUp = false">
                    <v-icon>close</v-icon>
                </v-btn>
                <span>关闭</span>
            </v-tooltip>
        </v-snackbar>
    </v-app>
</template>

<script type="text/javascript" src="./app.js"></script>
