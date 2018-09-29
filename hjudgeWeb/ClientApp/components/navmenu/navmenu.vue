<template>
    <div>
        <v-navigation-drawer :clipped="$vuetify.breakpoint.lgAndUp"
                             v-model="drawer"
                             fixed
                             app>
            <v-list dense>
                <template v-for="item in items">
                    <v-layout v-if="item.heading"
                              :key="item.heading"
                              row
                              align-center>
                        <v-flex xs6>
                            <v-subheader v-if="item.heading">
                                {{ item.heading }}
                            </v-subheader>
                        </v-flex>
                        <v-flex xs6 class="text-xs-center">
                            <a href="#!" class="body-2 black--text">EDIT</a>
                        </v-flex>
                    </v-layout>
                    <v-list-group v-else-if="item.children"
                                  v-model="item.model"
                                  :key="item.text"
                                  :prepend-icon="item.model ? item.icon : item['icon-alt']"
                                  append-icon="">
                        <v-list-tile slot="activator">
                            <v-list-tile-content>
                                <v-list-tile-title>
                                    <router-link :to="{ path: '{{item.link}}'}" v-if="item.link">
                                        {{ item.text }}
                                    </router-link>
                                    <a v-else>
                                        {{ item.text }}
                                    </a>
                                </v-list-tile-title>
                            </v-list-tile-content>
                        </v-list-tile>
                        <v-list-tile v-for="(child, i) in item.children"
                                     :key="i"
                                     :to="child.link">
                            <v-list-tile-action v-if="child.icon">
                                <v-icon>{{ child.icon }}</v-icon>
                            </v-list-tile-action>
                            <v-list-tile-content>
                                <v-list-tile-title>
                                    {{ child.text }}
                                </v-list-tile-title>
                            </v-list-tile-content>
                        </v-list-tile>
                    </v-list-group>
                    <v-list-tile v-else :key="item.text" :to="item.link">
                        <v-list-tile-action>
                            <v-icon>{{ item.icon }}</v-icon>
                        </v-list-tile-action>
                        <v-list-tile-content>
                            <v-list-tile-title>
                                {{ item.text }}
                            </v-list-tile-title>
                        </v-list-tile-content>
                    </v-list-tile>
                </template>
            </v-list>
        </v-navigation-drawer>
        <v-toolbar :clipped-left="$vuetify.breakpoint.lgAndUp"
                   color="primary"
                   dark
                   app
                   fixed>
            <v-toolbar-title style="width: 300px" class="ml-0 pl-3">
                <v-toolbar-side-icon @click.stop="drawer = !drawer"></v-toolbar-side-icon>
                <span class="hidden-sm-and-down">H::Judge</span>
            </v-toolbar-title>
            <v-spacer></v-spacer>
            <v-tooltip bottom>
                <v-btn @click="goback" slot="activator" icon>
                    <v-icon>keyboard_arrow_left</v-icon>
                </v-btn>
                <span>后退一步</span>
            </v-tooltip>
            <v-tooltip bottom>
                <v-btn @click="goforward" slot="activator" icon>
                    <v-icon>keyboard_arrow_right</v-icon>
                </v-btn>
                <span>前进一步</span>
            </v-tooltip>

            <v-menu offset-y>
                <v-tooltip slot="activator" bottom>
                    <v-btn slot="activator" icon>
                        <v-icon>account_circle</v-icon>
                    </v-btn>
                    <span>个人账户</span>
                </v-tooltip>
                <v-card light v-if="user.isSignedIn">
                    <v-card-title primary-title>
                        <h4>欢迎你，{{user.userName}}！</h4>
                    </v-card-title>
                    <v-card-actions>
                        <v-btn color="primary" to='/Account'>门户</v-btn>
                        <v-btn color="primary" @click="logout">退出</v-btn>
                    </v-card-actions>
                </v-card>
                <v-card light v-else>
                    <v-card-title primary-title>
                        <h4>欢迎来到 H::Judge！</h4>
                    </v-card-title>
                    <v-card-actions>
                        <div>
                            <v-dialog v-model="loginDialog" width="500">
                                <v-btn slot="activator" color="primary">登录</v-btn>
                                <login></login>
                            </v-dialog>
                            <v-dialog v-model="registerDialog" width="500">
                                <v-btn slot="activator" color="primary">注册</v-btn>
                                <register></register>
                            </v-dialog>
                        </div>
                    </v-card-actions>
                </v-card>
            </v-menu>
        </v-toolbar>
    </div>
</template>

<script src="./navmenu.js"></script>
