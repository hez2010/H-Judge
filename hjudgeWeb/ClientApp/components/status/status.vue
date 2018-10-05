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
                        <td><a @click="toResult(props.item.id)">{{ props.item.id }}</a></td>
                        <td>{{ props.item.judgeTime }}</td>
                        <td><a @click="toProblem(props.item.problemId)">{{ props.item.problemName }}</a></td>
                        <td><a @click="toUser(props.item.userId)">{{ props.item.userName }}</a></td>
                        <td>{{ props.item.language }}</td>
                        <td>{{ props.item.result }}</td>
                        <td>{{ props.item.fullScore }}</td>
                        <td v-if="$route.params.cid"><a @click="toContest(props.item.contestId)">{{ props.item.contestName }}</a></td>
                        <td v-if="$route.params.gid"><a @click="toGroup(props.item.groupId)">{{ props.item.groupName }}</a></td>
                    </template>
                    <template slot="no-data">
                        <p v-if="loading">正在加载...</p>
                        <p v-else>没有数据 :(</p>
                    </template>
                </v-data-table>
            </v-card-text>
            <v-card-actions>
                <v-layout justify-center align-center>
                    <v-pagination circle v-model="page" :length="pageCount" :total-visible="7"></v-pagination>
                </v-layout>
            </v-card-actions>
        </v-card>
    </v-container>
</template>

<script type="text/javascript" src="./status.js"></script>