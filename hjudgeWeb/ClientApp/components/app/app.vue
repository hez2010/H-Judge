updateFortune<template>
    <v-app :dark="darkTheme">
        <navmenu ref="navmenu" :themeIcon="themeIcon" :switchTheme="switchTheme" :user="userInfo" :getUserInfo="getUserInfo"></navmenu>
        <v-content>
            <router-view :user="userInfo" :getUserInfo="getUserInfo" :updateFortune="updateFortune"></router-view>
        </v-content>
        <v-footer class="pa-3 primary darken-2">
            <v-spacer></v-spacer>
            <span class="white--text ml-3">H::Judge {{ new Date().getFullYear() }}</span>
        </v-footer>
    </v-app>
</template>

<script type="text/javascript" src="./app.js"></script>
