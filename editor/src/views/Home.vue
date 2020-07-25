<template>
  <div class="home">
    <button @click="getTables()">get Tables</button>
    <button @click="clear()">RemoveAll</button>

    <button @click="addPage({params:{link}})">add page</button>
    <input v-model="link" />
    <div v-for="(table,id) in tables" :key="id">
      <br />
      <Table :table="table" />
      <!-- <h2>{{table.Name}}</h2>
      <div v-for="item in table.Properties" :key="item">{{ item }}</div>-->
    </div>
  </div>
</template>

<script>
// @ is an alias to /src
import { mapState, mapActions } from "vuex";
import { AgGridVue } from "ag-grid-vue";
import Table from "../components/Table";
export default {
  name: "Home",
  data: () => {
    return {
      link: "",
    };
  },
  components: { AgGridVue, Table },
  computed: mapState({
    tables: (state) => state.tables,
  }),
  methods: {
    ...mapActions(["getTables", "addPage", "clear"]),
  },
};
</script>
