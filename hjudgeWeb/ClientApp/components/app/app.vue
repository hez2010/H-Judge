<template>
    <v-app :dark="darkTheme">
        <navmenu v-on:register="test" :themeIcon="themeIcon" :switchTheme="switchTheme" :user="userInfo"></navmenu>
        <v-content>
            <router-view :user="userInfo"></router-view>
        </v-content>
        <v-footer class="pa-3 blue darken-2">
            <v-spacer></v-spacer>
            <span class="white--text ml-3">H::Judge {{ new Date().getFullYear() }}</span>
        </v-footer>
    </v-app>
</template>

<script src="./app.js"></script>