<template>
    <v-app>
        <navmenu v-on:register="test"></navmenu>
        <transition name="fade">
            <router-view v-on:register="test" keep-alive></router-view>
        </transition>
        <v-footer class="pa-3 blue darken-2">
            <v-spacer></v-spacer>
            <span class="white--text ml-3">H::Judge {{ new Date().getFullYear() }}</span>
        </v-footer>
    </v-app>
</template>

<script src="./app.js"></script>

<style>
    .fade-enter-active, .fade-leave-active {
        transition: opacity .5s;
    }

    .fade-enter, .fade-leave-to /* .fade-leave-active below version 2.1.8 */ {
        opacity: 0;
    }
</style>
