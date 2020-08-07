<template>
  <div class="space field">
    <div class="flex">
      {{Target.Property}}
      <plus :parent="Target" />
    </div>
    <div class="space"></div>
    <table-editor v-for="(target, id) in Target.Children" :key="id" :Target="target" />
  </div>
</template>

<script>
import plus from "./plus";

export default {
  name: "table-editor",
  props: {
    Target: {},
  },
  data() {
    return {};
  },
  components: { plus },
};
</script>

<style>
.space {
  margin-left: 1em;
}
.space-2 {
  margin-left: 2em;
}
.flex {
  display: flex;
}
.field {
  margin-top: 5px;
}
</style>