<template>
    <v-container>
        <v-card>
            <v-card-title primary-title>
                <h2>提交状态</h2>
            </v-card-title>
            <v-card-text>
                <v-data-table :headers="headers"
                              :items="statuses"
                              :custom-sort="sortRules"
                              hide-actions>
                    <template slot="items" slot-scope="props">
                        <td><router-link :to="{ path: '/Result/' + props.item.id }">{{ props.item.id }}</router-link></td>
                        <td>{{ props.item.judgeTime }}</td>
                        <td><router-link :to="{ path: '/ProblemDetails/' + getProblemRouteParams(props.item.problemId) }">{{ props.item.problemName }}</router-link></td>
                        <td><router-link :to="{ path: '/Account/' + props.item.userId }">{{ props.item.userName }}</router-link></td>
                        <td>{{ props.item.language }}</td>
                        <td>{{ props.item.result }}</td>
                        <td>{{ props.item.fullScore }}</td>
                        <td v-if="$route.params.cid"><router-link :to="{ path: '/ContestDetails/' + getContestRouteParams(props.item.contestId) }">{{ props.item.contestName }}</router-link></td>
                        <td v-if="$route.params.gid"><router-link :to="{ path: '/GroupDetails/' + props.item.groupId }">{{ props.item.groupName }}</router-link></td>
                    </template>
                    <template slot="no-data">
                        <p v-if="loading">正在加载...</p>
                        <p v-else>没有数据 :(</p>
                    </template>
                </v-data-table>
            </v-card-text>
            <v-card-actions>
                <v-layout justify-center align-center>
                    <v-pagination circle v-model="page" :length="pageCount" :total-visible="5"></v-pagination>
                </v-layout>
            </v-card-actions>
        </v-card>
    </v-container>
</template>

<script type="text/javascript" src="./status.js"></script>