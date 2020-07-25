<template>
  <div class="home">
    <button @click="getTables()">get Tables</button>
    <div v-for="(table,id) in tables" :key="id">
      <br />
      <h2>{{table.Name}}</h2>
      <div v-for="item in table.Properties" :key="item">{{ item }}</div>
    </div>
  </div>
</template>

<script>
// @ is an alias to /src
import { mapState, mapActions } from "vuex";
import { AgGridVue } from "ag-grid-vue";

export default {
  name: "Home",
  components: { AgGridVue },
  computed: mapState({
    tables: state => state.tables
  }),
  methods: {
    ...mapActions(["getTables"])
  }
};
</script>
